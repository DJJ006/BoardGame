using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CircusBoardManager : MonoBehaviour
{
    [Header("Board Setup")]
    public List<BoardTile> boardTiles = new List<BoardTile>();

    [Header("Players")]
    public List<PlayerToken> players = new List<PlayerToken>();

    public int stepsPerFace = 1;

    [Header("References")]
    public DiceRollCript diceRoll;
    public SoundEffectsScript soundEffects;

    [Header("Win UI")]
    public GameObject winPanel;
    public TMP_Text winMessageText;
    public TMP_Text winTimeText;
    public TMP_Text winMovesText;
    public TMP_Text winPointsText;

    [Header("Turn UI")]
    public TMP_Text currentPlayerText;
    public Image currentPlayerIcon;

    [Header("Player Icon Colors")]
    public Color player1Color = Color.red;
    public Color player2Color = Color.blue;
    public Color player3Color = Color.green;
    public Color player4Color = Color.yellow;

    [Header("Timer UI")]
    public TMP_Text timerText;           // assign in game scene

    private int _currentPlayerIndex;
    private bool _isProcessingTurn;
    private bool _gameOver;
    private bool[] _skipNextTurn;

    private int _totalDiceRolls;
    private float _elapsedTime;          // total seconds since game start

    private void Awake()
    {
        if (boardTiles.Count == 0)
        {
            Debug.LogError("CircusBoardManager: No board tiles assigned.");
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void Start()
    {
        boardTiles.Sort((a, b) => a.index.CompareTo(b.index));

        _skipNextTurn = new bool[players.Count];

        // assign per-player icon colors
        for (int i = 0; i < players.Count; i++)
        {
            Color c = GetColorForPlayerIndex(i);
            players[i].iconColor = c;
            players[i].Initialize(this, i);
            players[i].SetIconActive(i == 0); // only first player active at start
            MovePlayerToIndex(i, 0, instant: true);
        }

        _currentPlayerIndex = 0;
        _isProcessingTurn = false;
        _gameOver = false;
        _totalDiceRolls = 0;
        _elapsedTime = 0f;

        UpdateCurrentPlayerText();
        UpdateTimerText();  // start at 00:00
    }

    private Color GetColorForPlayerIndex(int index)
    {
        switch (index)
        {
            case 0: return player1Color;
            case 1: return player2Color;
            case 2: return player3Color;
            case 3: return player4Color;
            default: return Color.white;
        }
    }

    private void Update()
    {
        if (!_gameOver)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }

        if (_gameOver || diceRoll == null)
            return;

        if (diceRoll.isLanded && _isProcessingTurn)
        {
            _isProcessingTurn = false;
            HandleDiceResultForCurrentPlayer();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.FloorToInt(_elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void OnRollButtonPressed()
    {
        if (_gameOver || _isProcessingTurn)
            return;

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
        _totalDiceRolls++;

        diceRoll.ResetDice();
        diceRoll.RollNow();
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
            // play one step sound each time we enter a new tile
            if (soundEffects != null && players[playerIndex] != null)
            {
                // let the token trigger a step sound; falls back to SoundEffectsScript
                players[playerIndex].PlayStepSound();
            }

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

        // disable old player's icon highlight
        if (players != null && players.Count > 0)
        {
            players[_currentPlayerIndex].SetIconActive(false);
        }

        _currentPlayerIndex = (_currentPlayerIndex + 1) % players.Count;

        // enable new current player's icon highlight
        if (players != null && players.Count > 0)
        {
            players[_currentPlayerIndex].SetIconActive(true);
        }

        Debug.Log($"Next player: {_currentPlayerIndex}");
        UpdateCurrentPlayerText();
    }

    private void HandleWin(int playerIndex)
    {
        _gameOver = true;
        _isProcessingTurn = false;
        Debug.Log($"Player {playerIndex} wins!");

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
            soundEffects.PlayWin();
        }

        ShowWinPanel(playerIndex);
        SaveWinnerToLeaderboard(playerIndex);

        if (currentPlayerText != null)
        {
            currentPlayerText.text = string.Empty;
        }

        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.enabled = false;
        }
    }

    private void UpdateCurrentPlayerText()
    {
        if (players == null || players.Count == 0)
            return;

        if (currentPlayerText != null)
        {
            PlayerToken current = players[_currentPlayerIndex];
            string name = current != null ? current.name : $"Player {_currentPlayerIndex + 1}";
            currentPlayerText.text = $"{name} turn";
        }

        if (currentPlayerIcon != null)
        {
            currentPlayerIcon.enabled = true;
            currentPlayerIcon.color = GetColorForPlayerIndex(_currentPlayerIndex);
        }
    }

    private void ShowWinPanel(int playerIndex)
    {
        if (winPanel == null)
            return;

        winPanel.SetActive(true);

        string winnerName = players[playerIndex] != null
            ? players[playerIndex].name
            : $"Player {playerIndex + 1}";

        if (winMessageText != null)
        {
            winMessageText.text = $"{winnerName} wins!";
        }

        if (winTimeText != null)
        {
            int totalSeconds = Mathf.RoundToInt(_elapsedTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            winTimeText.text = $"Time: {minutes:00}:{seconds:00}";
        }

        if (winMovesText != null)
        {
            winMovesText.text = $"Moves: {_totalDiceRolls}";
        }

        int points = CalculatePoints(_elapsedTime, _totalDiceRolls);
        if (winPointsText != null)
        {
            winPointsText.text = $"Points: {points}";
        }
    }

    private int CalculatePoints(float timeSeconds, int moves)
    {
        // simple scoring example – tweak as you like
        int timePenalty = Mathf.RoundToInt(timeSeconds);
        int movePenalty = moves * 5;
        int baseScore = 2000;
        int score = baseScore - timePenalty - movePenalty;
        return Mathf.Max(score, 0);
    }

    private void SaveWinnerToLeaderboard(int playerIndex)
    {
        string winnerName = players[playerIndex] != null
            ? players[playerIndex].name
            : $"Player {playerIndex + 1}";

        int points = CalculatePoints(_elapsedTime, _totalDiceRolls);

        LeaderboardData leaderboard = LeaderboardData.Load();
        leaderboard.AddEntry(winnerName, points);
        leaderboard.Save();
    }

    // UI buttons on win panel
    public void OnWinPanelRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnWinPanelMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
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
