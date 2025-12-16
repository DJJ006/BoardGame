using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;          // whole pause UI
    public GameObject pauseButtonsPanel;   // Continue / Leaderboard / Settings / MainMenu
    public GameObject pauseLeaderboardPanel;
    public GameObject pauseSettingsPanel;

    [Header("Optional scripts")]
    public MainMenuLeaderboardUI leaderboardUI; // reuse to show scores
    public SettingsScript settingsScript;       // reuse settings panel logic

    [Header("Gameplay UI to disable while paused")]
    public List<Selectable> gameplayUiSelectables = new List<Selectable>();
    // e.g. drag your Roll button, animation buttons, etc. here in the Inspector

    private bool _isPaused;

    private void Awake()
    {
        SetPaused(false);
    }

    // Called by ESC key or Pause button in UI
    public void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    public void ContinueGame()
    {
        SetPaused(false);
    }

    public void OpenPauseLeaderboard()
    {
        ShowOnly(pauseLeaderboardPanel);
        if (leaderboardUI != null)
            leaderboardUI.Refresh();
    }

    public void OpenPauseSettings()
    {
        ShowOnly(pauseSettingsPanel);
    }

    public void OpenPauseMainButtons()
    {
        ShowOnly(pauseButtonsPanel);
    }

    public void QuitToMainMenu()
    {
        // Ensure time scale back to normal
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void SetPaused(bool pause)
    {
        _isPaused = pause;

        if (pausePanel != null)
            pausePanel.SetActive(pause);

        // Freeze or resume game time
        Time.timeScale = pause ? 0f : 1f;

        // Enable/disable gameplay UI buttons etc.
        SetGameplayUiInteractable(!pause);

        if (pause)
        {
            // default sub‑panel when opening pause
            ShowOnly(pauseButtonsPanel);
        }
    }

    private void SetGameplayUiInteractable(bool interactable)
    {
        if (gameplayUiSelectables == null)
            return;

        foreach (var sel in gameplayUiSelectables)
        {
            if (sel != null)
            {
                sel.interactable = interactable;
            }
        }
    }

    private void ShowOnly(GameObject panelToShow)
    {
        if (pauseButtonsPanel != null)
            pauseButtonsPanel.SetActive(panelToShow == pauseButtonsPanel);
        if (pauseLeaderboardPanel != null)
            pauseLeaderboardPanel.SetActive(panelToShow == pauseLeaderboardPanel);
        if (pauseSettingsPanel != null)
            pauseSettingsPanel.SetActive(panelToShow == pauseSettingsPanel);
    }
}
