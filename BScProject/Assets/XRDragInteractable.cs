using UnityEngine;
using UnityEngine.EventSystems;

public class XRDragInteractable : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform _rectTransform;
    [SerializeField] private Canvas _canvas;
    private Vector2 _startPosition;

    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("start drag");
        _startPosition = _rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, eventData.position, _canvas.worldCamera, out localPoint);

        Vector2 offset = localPoint - _startPosition;
        Vector2 newPosition = _rectTransform.anchoredPosition + offset;

        // Clamp the position within boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        _rectTransform.anchoredPosition = newPosition;
        _startPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
        // Handle any end-drag actions if needed
    }
}
