using System.Collections.Generic;
using UnityEngine;

public class GameSceneSetup : MonoBehaviour
{
    [Header("Prefabs in same order as CharacterSelectScript.characters")]
    public PlayerToken[] playerPrefabs;

    [Header("References in this scene")]
    public SaveLoadScript saveLoadScript;
    public CircusBoardManager boardManager;

    private void Awake()
    {
        // Load data saved in main menu (kept in case you use it for other things)
        if (saveLoadScript != null)
        {
            saveLoadScript.LoadGame();
        }

        if (boardManager.players == null)
        {
            boardManager.players = new List<PlayerToken>();
        }
        boardManager.players.Clear();

        // How many human players were chosen in CharacterSelectScript
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 1);
        playerCount = Mathf.Clamp(playerCount, 1, 4);

        for (int i = 0; i < playerCount; i++)
        {
            // Read each selected character index saved as "SelectedCharacter_i"
            int charIndex = PlayerPrefs.GetInt($"SelectedCharacter_{i}", 0);
            charIndex = Mathf.Clamp(charIndex, 0, playerPrefabs.Length - 1);

            PlayerToken playerInstance = Instantiate(playerPrefabs[charIndex]);

            // Position on board start (CircusBoardManager.Start will snap to tile 0)
            Vector3 p = playerInstance.transform.position;
            playerInstance.transform.position = new Vector3(p.x, 65f, p.z);

            // Read per‑player name saved from CharacterSelectScript
            string playerName = PlayerPrefs.GetString($"PlayerName_{i}", $"Player {i + 1}");
            playerInstance.name = playerName;

            // Mark as human controlled
            playerInstance.isAIControlled = false;
            playerInstance.PlayerIndex = i;

            boardManager.players.Add(playerInstance);
        }
    }
}
