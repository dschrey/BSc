using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SegmentObjectPositionOption : MonoBehaviour
{
    [SerializeField] private Image _segmentObjectImage;
    [SerializeField] private Toggle _segmentToggle;
    [SerializeField] private TMP_Text _distanceText;
    public int SegmentID;
    public Image SegmentCheckmark;
    public bool ObjectPositioned => DistanceValue != 0f;
    public float VerticalPosition = 0F;
    public float HorizontalPosition = 0F;
    public float DistanceValue = 0F;
    public Vector3 CanvasInteractablePosition;
    public Vector3 WorldObjectPosition;
    public GameObject SegmentObject { get; private set; }
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

    // private void Update() 
    // {
    //     if (_isSelected)
    //     {
    //         _distanceText.text = Distance.ToString("F2") + "m";
    //     }
    // }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentSelected(bool state)
    {
        if (state)
        {
            _parentObject.SelectedSegmentChanged.Invoke(SegmentID);
        }
        _isSelected = state;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void InitializeSegment(int segmentID, GameObject segmentObjectPrefab, Sprite objectSprite, GameObject segmentObjective, UISegmentObjectPosition parent)
    {
        SegmentID = segmentID;
        if (segmentID == 0)
        {
            _segmentToggle.isOn = true;
        }
        _segmentObjectPrefab = segmentObjectPrefab;
        SegmentObjective = segmentObjective;
        _segmentObjectImage.sprite = objectSprite;
        _parentObject = parent;
        _segmentToggle.group = parent.ToggleGroup;
    }

    public void CalculateDistance()
    {
        Vector3 objectPos = SegmentObject.transform.position;
        objectPos.y = 0f; 
        Vector3 objectivePos = SegmentObjective.transform.position;
        objectivePos.y = 0f; 
        DistanceValue = Vector3.Distance(objectPos, objectivePos);
        _distanceText.text = DistanceValue.ToString("F2") + "m";
    }


    public void SpawnSegmentObject(Transform parent)
    {
        if (SegmentObject == null)
        {
            SegmentObject = Instantiate(_segmentObjectPrefab, SegmentObjective.transform.position, Quaternion.identity, parent);
        }
        else
        {
            SegmentObject.SetActive(true);
        }
    }

    internal void DisableSegmentObject()
    {
        if (SegmentObject != null)
        {
            SegmentObject.SetActive(false);
        }
    }
}
