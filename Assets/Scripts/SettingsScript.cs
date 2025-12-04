using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple settings manager:
/// - change resolution (uses Screen.resolutions)
/// - change SFX volume (applies to a provided SFX AudioSource)
/// - change BGM (music) volume (applies to a provided music AudioSource)
/// Persist values to PlayerPrefs so changes survive runs.
/// Hook UI elements in the inspector: a TMP_Dropdown or legacy Dropdown for resolutions,
/// Slider(s) for volumes, and AudioSource references for SFX and Music.
/// 
/// Behavior:
/// - UI changes apply immediately (so player can preview)
/// - Changes are persisted only when SaveSettings() is called
/// - DiscardSettings() reverts UI and applied values to the last saved state
/// - ResetToDefaults() restores safe default values (applies them but does not persist)
/// </summary>
public class SettingsScript : MonoBehaviour
{
    // UI (assign at least one resolution dropdown: TMP or legacy)
    public TMP_Dropdown resolutionDropdownTMP;
    public Dropdown resolutionDropdownLegacy;

    // Volume sliders (assign in inspector)
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    // Audio sources to control (assign in inspector)
    // - sfxAudioSource should be the AudioSource used by SoundEffectsScript (or a shared SFX AudioSource)
    // - musicAudioSource should be the background music AudioSource (looping)
    public AudioSource sfxAudioSource;
    public AudioSource musicAudioSource;

    // Internal list of unique resolutions derived from Screen.resolutions
    private List<Resolution> availableResolutions = new List<Resolution>();

    // PlayerPrefs keys
    private const string PREF_RES_INDEX = "ResolutionIndex";
    private const string PREF_SFX_VOLUME = "SFXVolume";
    private const string PREF_BGM_VOLUME = "BGMVolume";

    // Default values used by ResetToDefaults
    private const float DEFAULT_SFX_VOLUME = 1f;
    private const float DEFAULT_BGM_VOLUME = 1f;

    // Last persisted values — used for discard logic
    private int savedResolutionIndex;
    private float savedSfxVolume;
    private float savedBgmVolume;

    void Awake()
    {
        PopulateResolutions();
        LoadAndApplySettings();
        HookUiEvents();
    }

    private void PopulateResolutions()
    {
        // Use Screen.resolutions and remove duplicates by width/height
        var resols = Screen.resolutions;
        availableResolutions = resols
            .Select(r => new Resolution { width = r.width, height = r.height, refreshRate = r.refreshRate })
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.OrderByDescending(r => r.refreshRate).First()) // prefer highest refresh for same size
            .OrderByDescending(r => r.width * r.height) // largest first
            .ToList();

        var options = availableResolutions.Select(r => $"{r.width} x {r.height}").ToList();

        if (resolutionDropdownTMP != null)
        {
            resolutionDropdownTMP.ClearOptions();
            resolutionDropdownTMP.AddOptions(options);
        }

        if (resolutionDropdownLegacy != null)
        {
            resolutionDropdownLegacy.ClearOptions();
            resolutionDropdownLegacy.AddOptions(options);
        }
    }

    private void LoadAndApplySettings()
    {
        // Load volumes (default 1.0)
        savedSfxVolume = PlayerPrefs.HasKey(PREF_SFX_VOLUME) ? PlayerPrefs.GetFloat(PREF_SFX_VOLUME) : DEFAULT_SFX_VOLUME;
        savedBgmVolume = PlayerPrefs.HasKey(PREF_BGM_VOLUME) ? PlayerPrefs.GetFloat(PREF_BGM_VOLUME) : DEFAULT_BGM_VOLUME;

        // Apply persisted values
        ApplySfxVolume(savedSfxVolume);
        ApplyMusicVolume(savedBgmVolume);

        // Set sliders if assigned (without triggering onValueChanged)
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(savedSfxVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(savedBgmVolume);

        // Load resolution index or pick current screen resolution index
        savedResolutionIndex = PlayerPrefs.HasKey(PREF_RES_INDEX) ? PlayerPrefs.GetInt(PREF_RES_INDEX) : -1;
        int indexToUse = savedResolutionIndex;

        if (indexToUse < 0 || indexToUse >= availableResolutions.Count)
        {
            // Try to find current resolution index
            indexToUse = availableResolutions.FindIndex(r => r.width == Screen.width && r.height == Screen.height);
            if (indexToUse == -1)
                indexToUse = 0; // fallback
            // Also treat this as the saved resolution if none stored
            savedResolutionIndex = indexToUse;
        }

        // Set UI dropdowns without triggering events
        if (resolutionDropdownTMP != null)
            resolutionDropdownTMP.SetValueWithoutNotify(indexToUse);
        if (resolutionDropdownLegacy != null)
            resolutionDropdownLegacy.SetValueWithoutNotify(indexToUse);

        // Apply resolution immediately to ensure the saved or detected one is used
        ApplyResolution(indexToUse);
    }

    private void HookUiEvents()
    {
        // Hook UI to apply-only handlers (do NOT persist on change)
        if (resolutionDropdownTMP != null)
        {
            resolutionDropdownTMP.onValueChanged.RemoveAllListeners();
            resolutionDropdownTMP.onValueChanged.AddListener(ApplyResolutionByIndexFromUI);
        }

        if (resolutionDropdownLegacy != null)
        {
            resolutionDropdownLegacy.onValueChanged.RemoveAllListeners();
            resolutionDropdownLegacy.onValueChanged.AddListener(ApplyResolutionByIndexFromUI);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(ApplySfxVolumeFromUI);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(ApplyMusicVolumeFromUI);
        }
    }

    // UI -> apply-only handlers (do not write PlayerPrefs)
    private void ApplyResolutionByIndexFromUI(int index)
    {
        ApplyResolution(index);
    }

    private void ApplySfxVolumeFromUI(float normalizedVolume)
    {
        ApplySfxVolume(normalizedVolume);
    }

    private void ApplyMusicVolumeFromUI(float normalizedVolume)
    {
        ApplyMusicVolume(normalizedVolume);
    }

    // Public API for explicit save flow

    /// <summary>
    /// Persist currently applied settings to PlayerPrefs.
    /// Hook this to your Save Button's OnClick in the Inspector.
    /// </summary>
    public void SaveSettings()
    {
        // resolution index (prefer TMP then legacy)
        int resIndex = -1;
        if (resolutionDropdownTMP != null)
            resIndex = resolutionDropdownTMP.value;
        else if (resolutionDropdownLegacy != null)
            resIndex = resolutionDropdownLegacy.value;

        if (resIndex >= 0 && resIndex < availableResolutions.Count)
        {
            PlayerPrefs.SetInt(PREF_RES_INDEX, resIndex);
            savedResolutionIndex = resIndex;
        }

        // SFX volume: prefer slider value, fallback to sfx audio source volume
        float sfxVol = DEFAULT_SFX_VOLUME;
        if (sfxVolumeSlider != null)
            sfxVol = sfxVolumeSlider.value;
        else if (sfxAudioSource != null)
            sfxVol = sfxAudioSource.volume;
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, Mathf.Clamp01(sfxVol));
        savedSfxVolume = Mathf.Clamp01(sfxVol);

        // Music volume: prefer slider value, fallback to music audio source volume
        float bgmVol = DEFAULT_BGM_VOLUME;
        if (musicVolumeSlider != null)
            bgmVol = musicVolumeSlider.value;
        else if (musicAudioSource != null)
            bgmVol = musicAudioSource.volume;
        PlayerPrefs.SetFloat(PREF_BGM_VOLUME, Mathf.Clamp01(bgmVol));
        savedBgmVolume = Mathf.Clamp01(bgmVol);

        PlayerPrefs.Save();
        Debug.Log("SettingsScript: Settings saved via SaveSettings().");
    }

    /// <summary>
    /// Revert UI and applied values to the last saved state (does not persist).
    /// Hook this to your Discard/Cancel Button's OnClick in the Inspector.
    /// </summary>
    public void DiscardSettings()
    {
        // Revert resolution UI and apply saved resolution
        int resIndex = Mathf.Clamp(savedResolutionIndex, 0, Math.Max(0, availableResolutions.Count - 1));
        if (resolutionDropdownTMP != null)
            resolutionDropdownTMP.SetValueWithoutNotify(resIndex);
        if (resolutionDropdownLegacy != null)
            resolutionDropdownLegacy.SetValueWithoutNotify(resIndex);
        ApplyResolution(resIndex);

        // Revert sliders (without invoking listeners) and apply volumes
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(savedSfxVolume);
        ApplySfxVolume(savedSfxVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(savedBgmVolume);
        ApplyMusicVolume(savedBgmVolume);

        Debug.Log("SettingsScript: Changes discarded, reverted to last saved settings.");
    }

    /// <summary>
    /// Reset settings to safe defaults and apply them.
    /// This does not persist — call SaveSettings() to persist.
    /// Hook this to a "Reset to Defaults" button if you have one.
    /// </summary>
    public void ResetToDefaults()
    {
        // Determine a sensible default resolution: prefer current native screen resolution if present
        int defaultIndex = availableResolutions.FindIndex(r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        if (defaultIndex == -1)
            defaultIndex = availableResolutions.Count > 0 ? 0 : -1;

        if (defaultIndex >= 0)
        {
            if (resolutionDropdownTMP != null)
                resolutionDropdownTMP.SetValueWithoutNotify(defaultIndex);
            if (resolutionDropdownLegacy != null)
                resolutionDropdownLegacy.SetValueWithoutNotify(defaultIndex);
            ApplyResolution(defaultIndex);
        }

        // Reset volumes to defaults (apply but do not save)
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(DEFAULT_SFX_VOLUME);
        ApplySfxVolume(DEFAULT_SFX_VOLUME);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(DEFAULT_BGM_VOLUME);
        ApplyMusicVolume(DEFAULT_BGM_VOLUME);

        Debug.Log("SettingsScript: Reset to defaults (applied, not saved).");
    }

    // Internal application helpers
    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= availableResolutions.Count)
            return;

        var r = availableResolutions[index];
        // preserve fullscreen state
        bool fullscreen = Screen.fullScreen;
        // use the resolution's width/height; use current fullscreen/window mode
        Screen.SetResolution(r.width, r.height, fullscreen);
        Debug.Log($"SettingsScript: Set resolution {r.width}x{r.height} (index {index})");
    }

    private void ApplySfxVolume(float normalizedVolume)
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = Mathf.Clamp01(normalizedVolume);

        // Optionally find SoundEffectsScript instances and update their audioSource.volume if not assigned
        if (sfxAudioSource == null)
        {
            var sfxScript = FindObjectOfType<SoundEffectsScript>();
            if (sfxScript != null && sfxScript.audioSource != null)
                sfxScript.audioSource.volume = Mathf.Clamp01(normalizedVolume);
        }
    }

    private void ApplyMusicVolume(float normalizedVolume)
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = Mathf.Clamp01(normalizedVolume);
        else
        {
            // try to find a plausible music source (Search for an AudioSource named "Music" or tagged)
            var found = FindObjectsOfType<AudioSource>().FirstOrDefault(a => string.Equals(a.gameObject.name, "Music", StringComparison.OrdinalIgnoreCase));
            if (found != null)
                found.volume = Mathf.Clamp01(normalizedVolume);
        }
    }
}
