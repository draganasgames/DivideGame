using UnityEngine;
using UnityEngine.UI;

public class NumberGridManager : MonoBehaviour
{
    [Header("Queue UI")]
    public Transform queuePanel;
    public NumberTile numberTilePrefab;

    [Header("Rules")]
    public int minValue = 2;
    public int maxValue = 24;

    public void FillStart(GameManager gm)
    {
        foreach (Transform c in queuePanel)
            Destroy(c.gameObject);

        for (int i = 0; i < 3; i++)
            SpawnNewRightmost(gm, true);
    }

    public void BackfillIfNeeded(GameManager gm)
    {
        if (queuePanel.childCount < 3)
            SpawnNewRightmost(gm, false);

        // refresh draggability (only the right can)
        UpdateQueueInteractivity();
    }

    private void SpawnNewRightmost(GameManager gm, bool firstTime)
    {
        var tile = Instantiate(numberTilePrefab, queuePanel);
        tile.InitFromQueue(gm, this, Random.Range(minValue, maxValue + 1));
        tile.SetRandomColor();

        if(!firstTime) tile.transform.SetSiblingIndex(0);
    }

    public void UpdateQueueInteractivity()
    {
        int n = queuePanel.childCount;
        for (int i = 0; i < n; i++)
        {
            var t = queuePanel.GetChild(i).GetComponent<NumberTile>();
            if (t == null) continue;

            bool isRightmost = (i == n - 1);
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = isRightmost; // only the right one can receive input
            cg.interactable = isRightmost;


            var rt = t.transform as RectTransform;
            rt.localScale = isRightmost ? Vector3.one * 1.1f : Vector3.one;
        }
    }

    public bool IsRightmost(NumberTile tile)
    {
        int n = queuePanel.childCount;
        if (tile.transform.parent != queuePanel) return false;
        return tile.transform.GetSiblingIndex() == n - 1;
    }

    public int GetRightmostValueOrMinusOne()
    {
        int n = queuePanel.childCount;
        if (n == 0) return -1;
        var t = queuePanel.GetChild(n - 1).GetComponent<NumberTile>();
        return t != null ? t.Value : -1;
    }
}