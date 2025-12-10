using System.Collections.Generic;
using UnityEngine;
// Add the following using directive if PlayerToken is in another namespace
// using YourNamespaceForPlayerToken;

// If PlayerToken is defined in another file in your project, add its using directive here.
// For example, if it's in the global namespace, no change is needed.
// If it's in a namespace like "Game.Tokens", uncomment and update the line below:
//using Game.Tokens;PlayerToken

public class CircusBoardManager : MonoBehaviour
{
    [Header("Board Setup")]
    [Tooltip("Ordered list of board tiles from start (0) to finish (last index).")]
    public List<BoardTile> boardTiles = new List<BoardTile>();

    [Header("Players")]
    [Tooltip("All player pieces in play, in turn order (0 = first player).")]
    public List<PlayerToken> players = new List<PlayerToken>();

    [Tooltip("How many board steps a dice value of '1' equals (usually 1).")]
    public int stepsPerFace = 1;

    [Header("References")]
    [Tooltip("Dice script used to roll and detect landed face.")]
    public DiceRollCript diceRoll;

    [Tooltip("Optional: sound effects for moves, win, etc.")]
    public SoundEffectsScript soundEffects;

    private int _currentPlayerIndex;
    private bool _isProcessingTurn;
    private bool _gameOver;

    // Track per-player state (lose next turn, etc.)
    private bool[] _skipNextTurn;

    private void Awake()
    {
        if (boardTiles.Count == 0)
        {
            Debug.LogError("CircusBoardManager: No board tiles assigned.");
        }
    }

    private void Start()
    {
        // Ensure tiles list is sorted by index
        boardTiles.Sort((a, b) => a.index.CompareTo(b.index));

        _skipNextTurn = new bool[players.Count];

        // Initialize all players to start tile (index 0)
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Initialize(this, i);
            MovePlayerToIndex(i, 0, instant: true);
        }

        _currentPlayerIndex = 0;
        _isProcessingTurn = false;
        _gameOver = false;
    }

    private void Update()
    {
        if (_gameOver || diceRoll == null)
            return;

        // When dice has landed and we are not already processing a turn,
        // process movement for the active player.
        if (diceRoll.isLanded && _isProcessingTurn)
        {
            // Prevent double-processing: immediately set flag false, then process
            _isProcessingTurn = false;
            HandleDiceResultForCurrentPlayer();
        }
    }

    /// <summary>
    /// Called externally (e.g., by a UI button) to start the current player's dice roll.
    /// </summary>
    public void OnRollButtonPressed()
    {
        if (_gameOver || _isProcessingTurn)
            return;

        PlayerToken currentPlayer = players[_currentPlayerIndex];

        // If this player must skip turn, handle and advance.
        if (_skipNextTurn[_currentPlayerIndex])
        {
            Debug.Log($"Player {_currentPlayerIndex} skips this turn.");
            _skipNextTurn[_currentPlayerIndex] = false;
            AdvanceToNextPlayer();
            return;
        }

        // Human player: wait for click on dice (your DiceRollCript already handles mouse click).
        // Here, we only mark that we are in a turn and let DiceRollCript roll on click.
        if (!currentPlayer.isAIControlled)
        {
            if (soundEffects != null)
            {
                soundEffects.OnDice();
            }

            _isProcessingTurn = true;
            diceRoll.firstThrow = false; // allow new roll
        }
        else
        {
            // AI: trigger a roll programmatically.
            StartCoroutine(AIRollRoutine());
        }
    }

    private System.Collections.IEnumerator AIRollRoutine()
    {
        PlayerToken currentPlayer = players[_currentPlayerIndex];

        if (soundEffects != null)
        {
            soundEffects.OnDice();
        }

        // Small delay to "think"
        yield return new WaitForSeconds(0.5f);

        _isProcessingTurn = true;

        // Simulate a click on dice (by calling ResetDice + direct roll)
        diceRoll.ResetDice();
        // Directly roll once
        var rBody = diceRoll.GetComponent<Rigidbody>();
        if (rBody != null)
        {
            // Use private method by reflection is ugly – instead,
            // expose a public method on DiceRollCript if you want.
            // For now, mimic what your Update does:
            diceRoll.firstThrow = true;
            diceRoll.isLanded = false;

            // Apply random forces similar to RollDice (you might adjust).
            float maxRandForceVal = 10f;
            float forceX = Random.Range(0, maxRandForceVal);
            float forceY = Random.Range(0, maxRandForceVal);
            float forceZ = Random.Range(0, maxRandForceVal);

            rBody.isKinematic = false;
            rBody.AddForce(Vector3.up * Random.Range(800, 1200));
            rBody.AddTorque(forceX, forceY, forceZ);
        }

        // Wait until SideDetectScript sets isLanded
        while (!diceRoll.isLanded)
        {
            yield return null;
        }

        // Now handle dice result
        _isProcessingTurn = false;
        HandleDiceResultForCurrentPlayer();
    }

    private void HandleDiceResultForCurrentPlayer()
    {
        if (_gameOver)
            return;

        int diceValue = ParseDiceFace(diceRoll.diceFaceNum);
        if (diceValue <= 0)
        {
            Debug.LogWarning($"Invalid dice value: {diceRoll.diceFaceNum}");
            AdvanceToNextPlayer();
            return;
        }

        int steps = diceValue * stepsPerFace;
        Debug.Log($"Player {_currentPlayerIndex} rolled {diceValue}, moving {steps} steps.");

        int startIndex = players[_currentPlayerIndex].BoardIndex;
        int targetIndex = Mathf.Clamp(startIndex + steps, 0, boardTiles.Count - 1);

        // Move token along path
        StartCoroutine(MoveAndResolveTileRoutine(_currentPlayerIndex, targetIndex));
    }

    private System.Collections.IEnumerator MoveAndResolveTileRoutine(int playerIndex, int targetIndex)
    {
        // Simple step-by-step move animation
        int fromIndex = players[playerIndex].BoardIndex;

        for (int i = fromIndex + 1; i <= targetIndex; i++)
        {
            MovePlayerToIndex(playerIndex, i, instant: false);
            yield return new WaitForSeconds(0.3f);
        }

        // Check if player reached or passed final tile
        if (targetIndex >= boardTiles.Count - 1)
        {
            HandleWin(playerIndex);
            yield break;
        }

        // Apply tile effect at landing tile
        BoardTile tile = boardTiles[targetIndex];
        bool loseNextTurn;
        int resultingIndex = tile.ApplyEffect(targetIndex, out loseNextTurn);

        if (loseNextTurn)
        {
            _skipNextTurn[playerIndex] = true;
        }

        if (resultingIndex != targetIndex)
        {
            // Teleport or extra move due to tile effect
            Debug.Log($"Player {playerIndex} tile effect: move to {resultingIndex}.");
            MovePlayerToIndex(playerIndex, resultingIndex, instant: false);
            yield return new WaitForSeconds(0.5f);

            if (resultingIndex >= boardTiles.Count - 1)
            {
                HandleWin(playerIndex);
                yield break;
            }
        }

        AdvanceToNextPlayer();
    }

    private void MovePlayerToIndex(int playerIndex, int tileIndex, bool instant)
    {
        tileIndex = Mathf.Clamp(tileIndex, 0, boardTiles.Count - 1);
        BoardTile tile = boardTiles[tileIndex];
        PlayerToken token = players[playerIndex];

        token.BoardIndex = tileIndex;

        Vector3 targetPos = tile.transform.position;
        if (instant)
        {
            token.transform.position = targetPos;
        }
        else
        {
            // Simple Lerp move
            token.StopAllCoroutines();
            token.StartCoroutine(token.MoveToPositionRoutine(targetPos, 0.25f));
        }
    }

    private void AdvanceToNextPlayer()
    {
        if (_gameOver)
            return;

        _currentPlayerIndex = (_currentPlayerIndex + 1) % players.Count;

        Debug.Log($"Next player: {_currentPlayerIndex}");
        // Optionally, auto-roll for AI, wait for UI click for human, etc.
        // For now, you manually call OnRollButtonPressed() from UI.
    }

    private void HandleWin(int playerIndex)
    {
        _gameOver = true;
        Debug.Log($"Player {playerIndex} wins!");

        if (soundEffects != null)
        {
            // Reuse any clip you like as "win", or extend SoundEffectsScript
            soundEffects.PlayButton();
        }

        // TODO: show win screen / change scene via SceneChanger, etc.
    }

    private int ParseDiceFace(string face)
    {
        int value;
        if (int.TryParse(face, out value))
        {
            return value;
        }

        // If side names are like "Side_1", "Side6", etc., extract digits
        string digits = "";
        foreach (char c in face)
        {
            if (char.IsDigit(c))
            {
                digits += c;
            }
        }

        if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out value))
        {
            return value;
        }

        return 0;
    }
}
