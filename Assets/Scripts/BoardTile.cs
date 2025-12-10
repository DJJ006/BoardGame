using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal,
    Forward,
    Back,
    LoseTurn,
    GoTo
}

public class BoardTile : MonoBehaviour
{
    [Tooltip("Index of this tile on the board path (0-based).")]
    public int index;

    [Tooltip("Type of tile effect.")]
    public TileType tileType = TileType.Normal;

    [Tooltip("Used for Forward/Back – number of steps. Positive for forward, negative for back.")]
    public int stepModifier = 0;

    [Tooltip("Used for GoTo – absolute index to teleport to.")]
    public int goToIndex = 0;

    [Tooltip("Optional: name to show in UI, etc.")]
    public string displayName = "Tile";

    /// <summary>
    /// Apply this tile's effect to a given player index.
    /// Returns the resulting board index (may be different from currentIndex).
    /// Additional flags (like loseTurn) are returned by out parameters.
    /// </summary>
    public int ApplyEffect(int currentIndex, out bool loseNextTurn)
    {
        loseNextTurn = false;

        switch (tileType)
        {
            case TileType.Normal:
                return currentIndex;

            case TileType.Forward:
                return Mathf.Max(0, currentIndex + Mathf.Abs(stepModifier));

            case TileType.Back:
                return Mathf.Max(0, currentIndex - Mathf.Abs(stepModifier));

            case TileType.LoseTurn:
                loseNextTurn = true;
                return currentIndex;

            case TileType.GoTo:
                return Mathf.Max(0, goToIndex);

            default:
                return currentIndex;
        }
    }
}
