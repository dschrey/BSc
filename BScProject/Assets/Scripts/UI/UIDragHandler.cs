using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _movementArea;
    private RectTransform _rectTransform;
    private bool _isDragging = false;
    public Vector2 _lastValidPosition;

    [SerializeField] private GameObject _objectIn3DSpace;
    private CanvasTo3DSpace _canvasTo3DSpace;
    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }

    private void Start() {
        _canvasTo3DSpace = _objectIn3DSpace.GetComponent<CanvasTo3DSpace>();
        _rectTransform = GetComponent<RectTransform>();

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.isValid)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _movementArea,
                eventData.pointerCurrentRaycast.screenPosition,
                eventData.enterEventCamera,
                out Vector2 pointInRectangle

            );
            Debug.Log($"Drag to {eventData.pointerCurrentRaycast.worldPosition} - {pointInRectangle}");
            Debug.Log($"Bounds: Y {_movementArea.rect.yMin} - {_movementArea.rect.yMax}; X {_movementArea.rect.xMin} - {_movementArea.rect.xMax}");
            Debug.Log($"Size to {_rectTransform.sizeDelta}");

            Vector2 clampedPosition = new(
                Mathf.Clamp(pointInRectangle.x, _movementArea.rect.xMin + _rectTransform.rect.width / 2, _movementArea.rect.xMax - _rectTransform.rect.width / 2),
                Mathf.Clamp(pointInRectangle.y, _movementArea.rect.yMin + _rectTransform.rect.height / 2, _movementArea.rect.yMax - _rectTransform.rect.height / 2)
            );
            _rectTransform.localPosition = clampedPosition;

            _canvasTo3DSpace.ScreenPositionToWorldSpace(clampedPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
    }

}
