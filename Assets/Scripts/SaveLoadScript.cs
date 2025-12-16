using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadScript : MonoBehaviour
{
    public string saveFileName = "Leaderboard.json";

    [Serializable]
    public class GameData
    {
        public int character;
        public string characterName;
        // Add other game data fields here
    }

    private GameData gameData = new GameData();

    // Public read-only accessors
    public int SelectedCharacterIndex => gameData.character;
    public string SelectedCharacterName => gameData.characterName;

    public void SaveGame(int character, string characterName)
    {
        gameData.character = character;
        gameData.characterName = characterName;
        string json = JsonUtility.ToJson(gameData);

        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);
        Debug.Log("Game Saved: " + Application.persistentDataPath + "/" + saveFileName);
    }

    public void LoadGame()
    {
        string filePath = Application.persistentDataPath + "/" + saveFileName;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"Game Loaded: character={gameData.character}, name={gameData.characterName}");
        }
        else
        {
            Debug.LogWarning("Save file not found: " + filePath);
            gameData = new GameData();
        }
    }
}
