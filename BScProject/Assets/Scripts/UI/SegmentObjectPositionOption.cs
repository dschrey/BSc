using UnityEngine;
using UnityEngine.UI;

public class SegmentObjectPositionOption : MonoBehaviour
{
    [SerializeField] private Image _segmentObjectImage;
    [SerializeField] private Toggle _segmentToggle;
    public Image SegmentCheckmark;
    public bool ObjectPositioned => VerticalDistance != 0f || HorizontalDistance != 0f || Distance != 0f;
    public float VerticalDistance = 0F;
    public float HorizontalDistance = 0F;
    public float Distance = 0F;
    public Vector3 CanvasPosition;
    public Vector3 WorldPosition;
    private UISegmentObjectPosition _parentObject;
    private int _segmentID;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _segmentToggle.onValueChanged.AddListener(OnSegmentSelected);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentSelected(bool state)
    {
        if (state)
            _parentObject.SelectedSegmentChanged.Invoke(_segmentID);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void InitializeSegment(int segmentID, Sprite objectSprite, UISegmentObjectPosition parent)
    {
        _segmentID = segmentID;
        _segmentObjectImage.sprite = objectSprite;
        _parentObject = parent;
        _segmentToggle.group = parent.ToggleGroup;
    }
}
