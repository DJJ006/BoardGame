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
        // Load data saved in main menu
        saveLoadScript.LoadGame();

        int charIndex = Mathf.Clamp(saveLoadScript.SelectedCharacterIndex, 0, playerPrefabs.Length - 1);
        string playerName = saveLoadScript.SelectedCharacterName;

        // Instantiate the selected character as Player 0
        PlayerToken playerInstance = Instantiate(playerPrefabs[charIndex]);

        // Force Y to 65 (keep X/Z from prefab)
        Vector3 p = playerInstance.transform.position;
        playerInstance.transform.position = new Vector3(p.x, 65f, p.z);

        playerInstance.name = string.IsNullOrEmpty(playerName) ? "Player1" : playerName;

        // Ensure boardManager has a players list and assign
        if (boardManager.players == null)
        {
            boardManager.players = new List<PlayerToken>();
        }
        boardManager.players.Clear();
        boardManager.players.Add(playerInstance);

        // Optionally you can add AI players here if you want:
        // PlayerToken ai = Instantiate(playerPrefabs[someIndex]);
        // ai.isAIControlled = true;
        // boardManager.players.Add(ai);
    }
}
