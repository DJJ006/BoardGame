using UnityEngine;

public class SoundEffectsScript : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;          // assign in inspector

    [Header("Clips")]
    public AudioClip diceHitClip;            // dice landing / hit sound
    public AudioClip walkStepClip;           // one footstep or short loop
    public AudioClip winClip;                // win / fanfare sound

    [Header("Button Clips")]
    public AudioClip buttonHoverClip;        // for OnButton (mouse over / selected)
    public AudioClip buttonClickClip;        // for ClickedButton (pressed)

    public void OnDice()                     // called when dice is rolled (existing)
    {
        PlayOneShot(buttonClickClip);
    }

    public void PlayDiceHit()
    {
        PlayOneShot(diceHitClip);
    }

    public void PlayWalkStep()
    {
        PlayOneShot(walkStepClip);
    }

    public void PlayWin()
    {
        PlayOneShot(winClip);
    }

    // === NEW: UI button sounds ===

    /// <summary>
    /// Call this from EventTrigger pointer enter / select, or from navigation highlight.
    /// </summary>
    public void OnButton()                   // hover / focus sound
    {
        PlayOneShot(buttonHoverClip);
    }

    /// <summary>
    /// Call this from Button OnClick for normal button presses.
    /// </summary>
    public void ClickedButton()
    {
        PlayOneShot(buttonClickClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
