using System.Collections;
using UnityEngine;

public class PlayerToken : MonoBehaviour
{
    [Tooltip("Is this player controlled by AI?")]
    public bool isAIControlled = false;

    [HideInInspector]
    public int PlayerIndex;

    [HideInInspector]
    public int BoardIndex;

    private CircusBoardManager _boardManager;
    private Animator _animator;

    // "Walk" must match the bool parameter name you use for walking
    private static readonly int IsMovingHash = Animator.StringToHash("Walk");
    // "Win" must match the Trigger parameter name in your Animator
    private static readonly int WinHash = Animator.StringToHash("Win");

    public void Initialize(CircusBoardManager boardManager, int playerIndex)
    {
        _boardManager = boardManager;
        PlayerIndex = playerIndex;
        BoardIndex = 0;

        // Cache animator if present
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public IEnumerator MoveToPositionRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Start walking animation
        if (_animator != null)
        {
            _animator.SetBool(IsMovingHash, true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;

        // Back to idle
        if (_animator != null)
        {
            _animator.SetBool(IsMovingHash, false);
        }
    }

    // Called by CircusBoardManager when this player wins
    public void PlayWinAnimation()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_animator != null)
        {
            _animator.ResetTrigger(WinHash);
            _animator.SetTrigger(WinHash);
        }
    }
}
