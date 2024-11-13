using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentObjectPositionOption : MonoBehaviour
{
    [SerializeField] private RawImage _segmentObjectImage;
    [SerializeField] private Toggle _segmentToggle;
    [SerializeField] private TMP_Text _distanceText;
    [SerializeField] private Image _segmentLabelImage;
    [SerializeField] private TMP_Text _segmentLabelText;
    public int SegmentID { get; private set; }
    public Image SegmentCheckmark;
    public bool ObjectPositioned => DistanceToObjective != 0f;
    public float DistanceToObjective = 0f;
    public float DistanceToRealObject = 0f;
    public Vector2 CanvasObjectPosition;
    public GameObject DraggableSegmentObject { get; private set; }
    public GameObject SegmentObjective { get; private set; }
    private GameObject _segmentObjectPrefab;
    private UISegmentObjectPosition _parentObject;
    private bool _isSelected = false;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable() 
    {
        _segmentToggle.onValueChanged.AddListener(OnSegmentSelected);
        SegmentObjective = null;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentSelected(bool state)
    {
        if (_parentObject == null)
            return;

        if (state)
        {
            _parentObject.SelectedSegmentChanged.Invoke(SegmentID);
        }
        _isSelected = state;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(int segmentID, Color segmentColor, ToggleGroup toggleGroup, RenderTexture objectRenderTexture, 
        GameObject segmentObjectPrefab, GameObject segmentObjective, UISegmentObjectPosition parent)
    {
        SegmentID = segmentID;
        _segmentLabelImage.color = segmentColor;
        if (_segmentLabelText != null)
        {
            _segmentLabelText.color = segmentColor;
            _segmentLabelText.text = (segmentID + 1).ToString();
        }
        if (toggleGroup == null)
        {
            Debug.LogError($"Toggle group parameter was not properly assigned.");
            return;
        }
        _segmentToggle.group = toggleGroup;
        if (_segmentToggle.group == null)
        {
            Debug.LogError($"Toggle group was not properly assigned.");
            return;
        }
        _segmentObjectPrefab = segmentObjectPrefab;
        SegmentObjective = segmentObjective;
        _segmentObjectImage.texture = objectRenderTexture;
        _parentObject = parent;
    }

    public void CalculateDistanceToObjective()
    {
        Vector3 objectPos = DraggableSegmentObject.transform.position;
        objectPos.y = 0f; 
        Vector3 objectivePos = SegmentObjective.transform.position;
        objectivePos.y = 0f; 
        DistanceToObjective = Vector3.Distance(objectPos, objectivePos);
        _distanceText.text = DistanceToObjective.ToString("F2") + "m";
    }

    public void CalculateDistanceToRealObject(GameObject realObject)
    {
        Vector3 objectPos = DraggableSegmentObject.transform.position;
        objectPos.y = 0f; 
        Vector3 realPos = realObject.transform.position;
        realPos.y = 0f; 
        DistanceToRealObject = Vector3.Distance(objectPos, realPos);
    }

    public void EnableDraggableSegmentObject(Transform parent)
    {
        if (DraggableSegmentObject == null)
        {
            DraggableSegmentObject = Instantiate(_segmentObjectPrefab, SegmentObjective.transform.position, Quaternion.identity, parent);
            DraggableSegmentObject.AddComponent<DraggableObject>();
            CanvasCameraHandler canvasCamera = FindObjectOfType<CanvasCameraHandler>();
            CanvasObjectPosition = canvasCamera.WorldCoordinatesToScreenSpace(DraggableSegmentObject.transform.position);
        }
        else
        {
            DraggableSegmentObject.SetActive(true);
        }
    }

    public void DisableDraggableSegmentObject()
    {
        if (DraggableSegmentObject != null)
        {
            DraggableSegmentObject.SetActive(false);
        }
    }

    public void Select()
    {
        _segmentToggle.isOn = true;
    }

    public float GetHorizontalValue()
    {
        return Math.Abs(SegmentObjective.transform.position.z - DraggableSegmentObject.transform.position.z);     
    }

    public float GetVerticalValue()
    {
        return Math.Abs(SegmentObjective.transform.position.x - DraggableSegmentObject.transform.position.x);     
    }

    public void CleanSegment()
    {
        Debug.Log($"CleanSegment :: Destroying drag object and segment {SegmentID}");
        Destroy(DraggableSegmentObject);
        Destroy(this);
    }
}
