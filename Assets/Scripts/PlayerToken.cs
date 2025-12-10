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

    public void Initialize(CircusBoardManager boardManager, int playerIndex)
    {
        _boardManager = boardManager;
        PlayerIndex = playerIndex;
        BoardIndex = 0;
    }

    public IEnumerator MoveToPositionRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }
}
