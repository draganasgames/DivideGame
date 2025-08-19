using UnityEngine;

public class TileSlot : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public NumberTile Occupant { get; private set; }

    private GridManager _grid;
    private Vector2 _snapOffset;

    public void Init(GridManager grid, int x, int y, Vector2 snapOffset)
    {
        _grid = grid;
        X = x;
        Y = y;
        _snapOffset = snapOffset;
        gameObject.tag = "Tile";
    }

    public bool IsEmpty => Occupant == null;

    public void Place(NumberTile tile)
    {
        Occupant = tile;
        tile.CurrentSlot = this;

        tile.transform.SetParent(transform, false);
        (tile.transform as RectTransform).anchoredPosition = _snapOffset;
    }

    public void Clear()
    {
        if (Occupant != null)
        {
            Occupant.CurrentSlot = null;
            Occupant = null;
        }
    }
}