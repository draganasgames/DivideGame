using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    public int width = 3;
    public int height = 3;
    public Transform boardParent;
    public TileSlot tileSlotPrefab;

    [Header("Slot Snap Offset")]
    public Vector2 snapOffset = new Vector2(100f, -100f);

    private TileSlot[,] slots;

    public void BuildGrid()
    {
        // If it already exists, don't make another road
        if (slots != null && slots.Length == width * height) return;

        // Clean up the old
        foreach (Transform child in boardParent)
            Destroy(child.gameObject);

        slots = new TileSlot[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var s = Instantiate(tileSlotPrefab, boardParent);
                s.Init(this, x, y, snapOffset);
                s.name = $"TileSlot {x},{y}";
                slots[x, y] = s;
            }
        }
    }

    public TileSlot GetSlot(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return slots[x, y];
    }

    public IEnumerable<TileSlot> GetNeighbors(TileSlot center)
    {
        if (center == null) yield break;

        yield return GetSlot(center.X + 1, center.Y); // right
        yield return GetSlot(center.X - 1, center.Y); // left
        yield return GetSlot(center.X, center.Y + 1); // up
        yield return GetSlot(center.X, center.Y - 1); // down
    }

    public TileSlot GetNeighbor(TileSlot center, Vector2Int direction)
    {
        return GetSlot(center.X + direction.x, center.Y + direction.y);
    }

    public bool IsFull()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (slots[x, y].Occupant == null) return false;
        return true;
    }

    // Can any tile on the board divide or be divided by the value  v?
    public bool AnyDivisibleWith(int v)
    {
        if (v <= 0) return false;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var occ = slots[x, y].Occupant;
                if (occ == null) continue;

                int a = occ.Value;

                if (a % v == 0 || v % a == 0)
                    return true;
            }
        }
        return false;
    }
}