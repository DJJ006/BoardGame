using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable leaderboard stored in PlayerPrefs as JSON.
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int points;
}

[Serializable]
public class LeaderboardData
{
    private const string PlayerPrefsKey = "Leaderboard";

    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    public void AddEntry(string name, int points)
    {
        if (string.IsNullOrEmpty(name))
            name = "Player";

        LeaderboardEntry existing = entries.Find(e => e.playerName == name);
        if (existing != null)
        {
            // keep best score
            if (points > existing.points)
            {
                existing.points = points;
            }
        }
        else
        {
            entries.Add(new LeaderboardEntry
            {
                playerName = name,
                points = points
            });
        }

        // sort descending by points
        entries.Sort((a, b) => b.points.CompareTo(a.points));
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    public static LeaderboardData Load()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            return new LeaderboardData();
        }

        string json = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new LeaderboardData();
        }

        try
        {
            return JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load leaderboard: " + e.Message);
            return new LeaderboardData();
        }
    }
}