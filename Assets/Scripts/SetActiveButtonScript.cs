using UnityEngine;

public class SetActiveButtonScript : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainButtonsPanel;     // Start / Leaderboard / Settings / Quit
    public GameObject characterSelectPanel; // already used
    public GameObject settingsPanel;        // already used

    [Header("NEW: Leaderboard")]
    public GameObject leaderboardPanel;     // new leaderboard panel

    // called by Start button
    public void OpenCharacterSelect()
    {
        ShowOnly(characterSelectPanel);
    }

    // called by Settings button
    public void OpenSettings()
    {
        ShowOnly(settingsPanel);
    }

    // called by Leaderboard button
    public void OpenLeaderboard()
    {
        ShowOnly(leaderboardPanel);
    }

    // called by "Back" buttons inside CharacterSelect, Settings, Leaderboard
    public void OpenMainButtons()
    {
        ShowOnly(mainButtonsPanel);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        if (mainButtonsPanel != null)    mainButtonsPanel.SetActive(panelToShow == mainButtonsPanel);
        if (characterSelectPanel != null) characterSelectPanel.SetActive(panelToShow == characterSelectPanel);
        if (settingsPanel != null)      settingsPanel.SetActive(panelToShow == settingsPanel);
        if (leaderboardPanel != null)   leaderboardPanel.SetActive(panelToShow == leaderboardPanel);
    }
}
