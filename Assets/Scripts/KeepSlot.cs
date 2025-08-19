using UnityEngine;

public class KeepSlot : MonoBehaviour
{
    public NumberTile Tile { get; private set; }
    public bool HasTile => Tile != null;

    private float offset = 125f;

    public bool TryStore(NumberTile t)
    {
        if (HasTile) return false;
        Tile = t;
        t.transform.SetParent(transform, false);
        (t.transform as RectTransform).anchoredPosition = new Vector2(offset, -offset);
        t.InKeep = true;
        return true;
    }

    public NumberTile TakeOut()
    {
        var t = Tile;
        Tile = null;
        if (t != null) t.InKeep = false;
        return t;
    }

    public void Clear()
    {
        if (Tile != null)
        {
            Destroy(Tile.gameObject);
            Tile = null;
        }
    }
}