using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private CanvasGroup _cg;

    [SerializeField] private List<Color> Colors;

    public int Value { get; private set; }
    public bool IsPlaced { get; private set; }
    public bool InKeep { get; set; }
    public TileSlot CurrentSlot { get; set; }

    // refs
    private Canvas mainCanvas;
    private GameManager game;
    private NumberGridManager numberGridManager;

    // drag
    private Vector3 startWorldPos;
    private Transform startParent;

    // is it "draggable" while in the queue
    private bool queueDraggable = false;

    public void InitFromQueue(GameManager gm, NumberGridManager q, int value)
    {
        game = gm;
        numberGridManager = q;
        SetValue(value);
        EnsureCanvasGroup();
        numberGridManager.UpdateQueueInteractivity(); // initially, decide who is allowed to drag
    }

    private void Awake()
    {
        mainCanvas = FindObjectOfType<Canvas>();
        EnsureCanvasGroup();
    }

    private void EnsureCanvasGroup()
    {
        if (_cg == null) _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetValue(int v)
    {
        Value = v;
        if (_text != null) _text.text = Value.ToString();
    }

    public void SetQueueDraggable(bool canDrag)
    {
        queueDraggable = canDrag;
    }

    public void LockInteraction()
    {
        IsPlaced = true;
        // disable further capture in the EventSystem
        _cg.blocksRaycasts = false;
        _cg.interactable = false;
    }

    public void SetRandomColor()
    {
        if (Colors == null || Colors.Count == 0) return;
        Color randomColor = Colors[Random.Range(0, Colors.Count)];

        GetComponent<Image>().color = randomColor;
    }

    // ===== Drag & Drop =====
    public void OnPointerDown(PointerEventData eventData)
    {
        // If it's already on the board - don't touch it
        if (IsPlaced) return;

        if (InKeep)
        {
            var keep = transform.parent.GetComponent<KeepSlot>();
            if (keep != null)
            {
                keep.TakeOut();
                InKeep = false;
            }
        }

        startWorldPos = transform.position;
        startParent = transform.parent;
        transform.SetParent(mainCanvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsPlaced) return;

        if (startParent == null) return;

        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPoint
        );
        transform.position = worldPoint;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPlaced) return;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool placed = false;
        bool kept = false;

        foreach (var r in results)
        {
            // 1) It falls on the Tile (board)
            if (r.gameObject.CompareTag("Tile"))
            {
                var slot = r.gameObject.GetComponent<TileSlot>();
                if (slot != null && slot.IsEmpty)
                {
                    transform.SetParent(slot.transform, false);

                    var rt = transform as RectTransform;
                    rt.localScale = Vector3.one;

                    game.OnNumberPlaced(this, slot);
                    placed = true;
                    break;
                }
            }
            // 2) It falls on Keep
            else if (r.gameObject.CompareTag("Keep"))
            {
                var keep = r.gameObject.GetComponent<KeepSlot>();
                if (keep != null)
                {
                    if (keep.HasTile == false)
                    {
                        keep.TryStore(this);
                        kept = true;

                        if (startParent != null && numberGridManager != null && startParent == numberGridManager.queuePanel)
                            numberGridManager.BackfillIfNeeded(game);

                        break;
                    }
                }
            }
        }

        if (!placed && !kept)
        {
            // put it back where it was
            transform.SetParent(startParent, true);
            transform.position = startWorldPos;
        }

        // If the tile is moved from the queue panel - update the interactivity
        if (numberGridManager != null) numberGridManager.UpdateQueueInteractivity();
    }
}