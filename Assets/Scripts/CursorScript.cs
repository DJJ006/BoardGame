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

    // Update is called once per frame
    public void DefaultCursor()
    {
        Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);  
    }

    public void OnButtonCursor()
    {
        Cursor.SetCursor(cursors[1], Vector2.zero, CursorMode.Auto);
    }

    public void ButtonClickedCursor()
    {
        Cursor.SetCursor(cursors[2], Vector2.zero, CursorMode.Auto);
    }

    public void onPropCursor()
    {
        Cursor.SetCursor(cursors[3], Vector2.zero, CursorMode.Auto);
    }

    public void AttentionCursor()
    {
        Cursor.SetCursor(cursors[4], Vector2.zero, CursorMode.Auto);
    }

}
