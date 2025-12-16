using System.Collections.Generic;
using UnityEngine;

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

        // Skip-turn logic
        if (_skipNextTurn[_currentPlayerIndex])
        {
            Debug.Log($"Player {_currentPlayerIndex} skips this turn.");
            _skipNextTurn[_currentPlayerIndex] = false;
            AdvanceToNextPlayer();
            return;
        }

        if (soundEffects != null)
        {
            soundEffects.OnDice();
        }

        _isProcessingTurn = true;

        // RESET and ROLL THE DICE PHYSICALLY
        diceRoll.ResetDice();   // go back to start position, clear flags
        diceRoll.RollNow();     // actually apply forces to the rigidbody
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

        Vector3 tilePos = tile.transform.position;

        // <<< Force your desired Y here >>>
        const float playerHeightY = 65f;               // adjust as needed
        Vector3 targetPos = new Vector3(tilePos.x, playerHeightY, tilePos.z);

        if (instant)
        {
            token.transform.position = targetPos;
        }
        else
        {
            token.StopAllCoroutines();
            token.StartCoroutine(token.MoveToPositionRoutine(targetPos, 0.65f));
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

        // Play win animation on the winning token
        if (players != null && playerIndex >= 0 && playerIndex < players.Count)
        {
            PlayerToken winner = players[playerIndex];
            if (winner != null)
            {
                winner.PlayWinAnimation();
            }
        }

        if (soundEffects != null)
        {
            // Reuse any clip you like as "win", or extend SoundEffectsScript
            soundEffects.PlayButton();
        }

        // TODO: show win screen / change scene via SceneChanger, etc.
    }

    private int ParseDiceFace(string face)
    {
        if (int.TryParse(face, out int value))
        {
            // Clamp between 1 and 6
            value = Mathf.Clamp(value, 1, 6);
            return value;
        }

        // If something went wrong, log and return 0 (invalid)
        Debug.LogWarning($"Could not parse dice face '{face}'");
        return 0;
    }

    /// <summary>
    /// Play idle animation on the current player's token.
    /// Hook this to your "Idle" UI button.
    /// </summary>
    public void PlayCurrentPlayerIdle()
    {
        if (_gameOver || players == null || players.Count == 0) return;

        PlayerToken current = players[_currentPlayerIndex];
        if (current != null)
        {
            current.PlayIdleAnimation();
        }
    }

    /// <summary>
    /// Play walk animation on the current player's token.
    /// Hook this to your "Walk" UI button.
    /// </summary>
    public void PlayCurrentPlayerWalk()
    {
        if (_gameOver || players == null || players.Count == 0) return;

        PlayerToken current = players[_currentPlayerIndex];
        if (current != null)
        {
            current.PlayWalkAnimation();
        }
    }

    /// <summary>
    /// Play dying animation on the current player's token.
    /// Hook this to your "Die" UI button.
    /// </summary>
    public void PlayCurrentPlayerDie()
    {
        if (_gameOver || players == null || players.Count == 0) return;

        PlayerToken current = players[_currentPlayerIndex];
        if (current != null)
        {
            current.PlayDieAnimation();
        }
    }
}
