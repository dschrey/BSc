using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _movementArea;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    [SerializeField] private Image _imgDragHint;
    public UnityEvent WorldPositionChanged;
    public UnityEvent SegmentObjectPositioned;
    [SerializeField] private RectTransform _rectTransform;
    public DraggableObject DraggableWorldObject;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();

        _sliderhorizontalPosition.onValueChanged.AddListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.AddListener(OnVerticalPositionChanged);
    }

    private void OnDisable()
    {
        _sliderhorizontalPosition.onValueChanged.RemoveListener(OnHorizontalPositionChanged);
        _sliderverticalPosition.onValueChanged.RemoveListener(OnVerticalPositionChanged);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    public void OnDrag(PointerEventData eventData)
    {
        if (DraggableWorldObject == null)
            return;

        if (_imgDragHint.enabled)
            _imgDragHint.enabled = false;
        if (eventData.pointerCurrentRaycast.isValid)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _movementArea,
                eventData.pointerCurrentRaycast.screenPosition,
                eventData.enterEventCamera,
                out Vector2 pointInRectangle

            );

            Vector2 clampedCanvasPosition = new(
                Mathf.Clamp(pointInRectangle.x, _movementArea.rect.xMin, _movementArea.rect.xMax),
                Mathf.Clamp(pointInRectangle.y, _movementArea.rect.yMin, _movementArea.rect.yMax)
            );
            _rectTransform.localPosition = clampedCanvasPosition;

            UpdateSliderValues();
            DraggableWorldObject.DragObjectIn3DSpace(clampedCanvasPosition);
            WorldPositionChanged?.Invoke();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SegmentObjectPositioned.Invoke();
    }


    private void OnHorizontalPositionChanged(float value)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;
        newPosition.x = value;
        _rectTransform.anchoredPosition = newPosition;
        UpdateWorldObjectPosition(newPosition);
    }

    private void OnVerticalPositionChanged(float value)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;
        newPosition.y = value;
        _rectTransform.anchoredPosition = newPosition;
        UpdateWorldObjectPosition(newPosition);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void PositionCanvasObject(Vector3 screenPosition)
    {
        _rectTransform.anchoredPosition = screenPosition;
    }

    public void UpdateSliderValues()
    {
        _sliderhorizontalPosition.value = _rectTransform.anchoredPosition.x;
        _sliderverticalPosition.value = _rectTransform.anchoredPosition.y;
    }

    private void UpdateWorldObjectPosition(Vector2 newPosition)
    {
        if (DraggableWorldObject != null)
        {
            DraggableWorldObject.DragObjectIn3DSpace(newPosition);
            WorldPositionChanged?.Invoke();
            SegmentObjectPositioned?.Invoke();
        }
    }

    public Vector2 GetCurrentPosition()
    {
        return _rectTransform.anchoredPosition;
    }

    
    public void AdjustHorizontalSlider(float adjustmentValue)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;

        float newSliderValue;
        if (adjustmentValue > 0)
        {
            newSliderValue = Mathf.Min(_sliderhorizontalPosition.value + adjustmentValue, _sliderhorizontalPosition.maxValue);
        }
        else
        {
            newSliderValue = Mathf.Max(_sliderhorizontalPosition.value + adjustmentValue, _sliderhorizontalPosition.minValue);
        }

        newPosition.x += adjustmentValue;

        _sliderhorizontalPosition.value = newSliderValue;
        _rectTransform.anchoredPosition = newPosition;
        UpdateWorldObjectPosition(newPosition);
    }
    
    public void AdjustVerticalSlider(float adjustmentValue)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;

        float newSliderValue;
        if (adjustmentValue > 0)
        {
            newSliderValue = Mathf.Min(_sliderverticalPosition.value + adjustmentValue, _sliderverticalPosition.maxValue);
        }
        else
        {
            newSliderValue = Mathf.Max(_sliderverticalPosition.value + adjustmentValue, _sliderverticalPosition.minValue);
        }

        newPosition.y += adjustmentValue;

        _sliderverticalPosition.value = newSliderValue;
        _rectTransform.anchoredPosition = newPosition;
        UpdateWorldObjectPosition(newPosition);
    }

}
