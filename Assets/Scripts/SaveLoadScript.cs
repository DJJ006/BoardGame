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
        public String characterName;
        // Add other game data fields here
    }

    private GameData gameData = new GameData();
    
    public void SaveGame(int character, String characterName)
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

            //use gamedata to restore game state
        }
        else
        {
                       Debug.LogWarning("Save file not found: " + filePath);
        }
    }

}
