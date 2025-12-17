using System.Collections;
using UnityEngine;

public class PlayerToken : MonoBehaviour
{
    public bool isAIControlled;
    public int PlayerIndex;
    public int BoardIndex;

    private CircusBoardManager _boardManager;
    private Animator _animator;

    [Header("Player Icon")]
    public SpriteRenderer iconRenderer;   // world-space icon above head
    public Color iconColor = Color.white; // per-player color, set by GameSceneSetup

    // Make sure this type matches the component you drag in the Inspector
    public SoundEffectsScript soundEffects;

    // Shared Animator parameters
    private const string WalkParamName = "Walk";
    private const string IdleParamName = "Idle";
    private const string WinParamName  = "Win";

    private static readonly int WalkHash = Animator.StringToHash(WalkParamName);
    private static readonly int IdleHash = Animator.StringToHash(IdleParamName);
    private static readonly int WinHash  = Animator.StringToHash(WinParamName);

    private Coroutine _buttonAnimCoroutine;

    public void Initialize(CircusBoardManager boardManager, int playerIndex)
    {
        _boardManager = boardManager;
        PlayerIndex = playerIndex;
        EnsureAnimator();

        // apply icon color (set from GameSceneSetup)
        if (iconRenderer != null)
        {
            iconRenderer.color = iconColor;
        }
    }

    public IEnumerator MoveToPositionRoutine(Vector3 targetPosition, float duration)
    {
        EnsureAnimator();

        // movement overrides any active button / win animation
        if (_buttonAnimCoroutine != null)
        {
            StopCoroutine(_buttonAnimCoroutine);
            _buttonAnimCoroutine = null;
        }

        if (_animator != null)
        {
            _animator.ResetTrigger(WinHash);

            // Walk = true, Idle = false while moving
            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, true);
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, false);
        }

        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, targetPosition, t);

            // play step sound on each tile move start, not every frame
            // (so do NOT call here in every frame)
            yield return null;
        }

        transform.position = targetPosition;

        // back to idle after movement
        if (_animator != null)
        {
            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, false);
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, true);
        }
    }

    public void PlayWinAnimation()
    {
        EnsureAnimator();
        if (_animator == null) return;

        // treat win like the button: 3s, then back to idle
        StartButtonAnimationRoutine(AnimationMode.Win);
    }

    // ===== BUTTON CONTROLLED ANIMATIONS (3 seconds) =====

    public void PlayIdleAnimation()
    {
        EnsureAnimator();
        if (_animator == null) return;
        StartButtonAnimationRoutine(AnimationMode.Idle);
    }

    public void PlayWalkAnimation()
    {
        EnsureAnimator();
        if (_animator == null) return;
        StartButtonAnimationRoutine(AnimationMode.Walk);
    }

    public void PlayDieAnimation()
    {
        EnsureAnimator();
        if (_animator == null) return;
        StartButtonAnimationRoutine(AnimationMode.Win);
    }

    private enum AnimationMode { Idle, Walk, Win }

    private void StartButtonAnimationRoutine(AnimationMode mode)
    {
        if (_animator == null) return;

        // validate parameters once per call
        switch (mode)
        {
            case AnimationMode.Idle:
                if (!_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                    return;
                break;
            case AnimationMode.Walk:
                if (!_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                    return;
                break;
            case AnimationMode.Win:
                if (!_animator.HasParameterOfType(WinParamName, AnimatorControllerParameterType.Trigger))
                    return;
                break;
        }

        if (_buttonAnimCoroutine != null)
        {
            StopCoroutine(_buttonAnimCoroutine);
        }
        _buttonAnimCoroutine = StartCoroutine(ButtonAnimationRoutine(mode));
    }

    private IEnumerator ButtonAnimationRoutine(AnimationMode mode)
    {
        if (_animator == null)
            yield break;

        // 1) Enter requested state
        if (mode == AnimationMode.Idle)
        {
            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, false);
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, true);
        }
        else if (mode == AnimationMode.Walk)
        {
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, false);
            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, true);
        }
        else if (mode == AnimationMode.Win)
        {
            // Important: set locomotion bools so that a transition
            // OUT of Win back to Idle is possible
            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, false);
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, true);

            _animator.ResetTrigger(WinHash);
            _animator.SetTrigger(WinHash);
        }

        // 2) Wait 3 seconds
        const float duration = 3f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // 3) Force “back to idle” state (for any mode)
        if (_animator != null)
        {
            _animator.ResetTrigger(WinHash);

            if (_animator.HasParameterOfType(WalkParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(WalkHash, false);
            if (_animator.HasParameterOfType(IdleParamName, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IdleHash, true);
        }

        _buttonAnimCoroutine = null;
    }

    private void EnsureAnimator()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    // Call this from CircusBoardManager when starting a move between tiles:
    public void PlayStepSound()
    {
        if (soundEffects == null)
        {
            soundEffects = FindObjectOfType<SoundEffectsScript>();
        }

        if (soundEffects != null)
        {
            soundEffects.PlayWalkStep();
        }
    }

    // called by CircusBoardManager when turn changes
    public void SetIconActive(bool active)
    {
        if (iconRenderer != null)
        {
            iconRenderer.enabled = active;
        }
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
    {
        if (self == null) return false;
        foreach (var p in self.parameters)
        {
            if (p.type == type && p.name == name)
                return true;
        }
        return false;
    }
}
