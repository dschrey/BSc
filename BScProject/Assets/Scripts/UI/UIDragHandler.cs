using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragHandler : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform _movementArea;
    [SerializeField] private Slider _sliderhorizontalPosition;
    [SerializeField] private Slider _sliderverticalPosition;
    public UnityEvent<Vector3> WorldPositionChanged;
    private RectTransform _rectTransform;
    private GameObject _objectInWorldSpace;
    private DraggableObject _draggableObject;

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
        if (_objectInWorldSpace == null)
            return;
        if (eventData.pointerCurrentRaycast.isValid)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _movementArea,
                eventData.pointerCurrentRaycast.screenPosition,
                eventData.enterEventCamera,
                out Vector2 pointInRectangle

            );

            Vector2 clampedPosition = new(
                Mathf.Clamp(pointInRectangle.x, _movementArea.rect.xMin + _rectTransform.rect.width / 2, _movementArea.rect.xMax - _rectTransform.rect.width / 2),
                Mathf.Clamp(pointInRectangle.y, _movementArea.rect.yMin + _rectTransform.rect.height / 2, _movementArea.rect.yMax - _rectTransform.rect.height / 2)
            );
            _rectTransform.localPosition = clampedPosition;

            Vector3 worldPosition = _draggableObject.DragObjectIn3DSpace(clampedPosition);
            UpdateSliderValues();
            WorldPositionChanged?.Invoke(worldPosition);
        }
    }

    private void OnHorizontalPositionChanged(float value)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;
        newPosition.x = value + (_movementArea.rect.width / 2); 
        _rectTransform.anchoredPosition = newPosition;
    }

    private void OnVerticalPositionChanged(float value)
    {
        Vector2 newPosition = _rectTransform.anchoredPosition;
        newPosition.y = value + (_movementArea.rect.height / 2);
        _rectTransform.anchoredPosition = newPosition;   
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SetWorldObject(GameObject draggableObject)
    {
        _objectInWorldSpace = draggableObject;
        _draggableObject = _objectInWorldSpace.GetComponent<DraggableObject>();

        CanvasCameraHandler canvasCameraHandler = FindObjectOfType<CanvasCameraHandler>();
        Vector3 screenPosition = canvasCameraHandler.WorldCoordinatesToScreenSpace(_objectInWorldSpace.transform.position);
        Debug.Log($"Moved {this} to {screenPosition}");
        _rectTransform.localPosition = screenPosition;
    }

    public void UpdateSliderValues()
    {
        Vector2 currentPosition = _rectTransform.anchoredPosition;

        _sliderhorizontalPosition.value = currentPosition.x - (_movementArea.rect.width / 2);
        _sliderverticalPosition.value = currentPosition.y - (_movementArea.rect.height / 2);
    }

}
