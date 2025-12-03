using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    public Texture2D[] cursors;

    void Start()
    {
        DefaultCursor();
    }

    void OnValidate()
    {
        // Ensure array exists and has at least 5 slots so inspector can display them.
        if (cursors == null)
            cursors = new Texture2D[5];
        else if (cursors.Length < 5)
            System.Array.Resize(ref cursors, 5);
    }

    // centralized safe setter with fallbacks and logged warnings
    private void SetCursorSafe(int index)
    {
        if (cursors == null)
        {
            Debug.LogWarning("CursorScript: 'cursors' array is null. Resetting to default cursor.");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (index < 0 || index >= cursors.Length)
        {
            Debug.LogWarning($"CursorScript: requested index {index} is out of range (length={cursors.Length}). Using default.");
            if (cursors.Length > 0 && cursors[0] != null)
                Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        if (cursors[index] == null)
        {
            Debug.LogWarning($"CursorScript: cursors[{index}] is null. Falling back to default.");
            if (cursors.Length > 0 && cursors[0] != null)
                Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(cursors[index], Vector2.zero, CursorMode.Auto);
    }

    public void DefaultCursor()
    {
        SetCursorSafe(0);
    }

    public void OnButtonCursor()
    {
        SetCursorSafe(1);
    }

    public void ButtonClickedCursor()
    {
        SetCursorSafe(2);
    }

    public void onPropCursor()
    {
        SetCursorSafe(3);
    }

    public void AttentionCursor()
    {
        SetCursorSafe(4);
    }
}
