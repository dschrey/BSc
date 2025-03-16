using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectPosition : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private PathLayoutCreator _pathPreviewCreator;

    [Header("Object Positioning")]
    [SerializeField] private Slider _sliderDistanceValue;
    [SerializeField] private LineController _lineRender;

    [Header("Object Info")]
    [SerializeField] private TMP_Text _textSegmentID;
    [SerializeField] private TMP_Text _textDistanceValue;
    [SerializeField] private TMP_Text _textDistanceValueBig;
    [SerializeField] private RawImage _objectPreview;

    [Header("Object Selection")]
    [SerializeField] private GameObject _selectionObjectPrefab;
    [SerializeField] private Transform _positionImage;
    [SerializeField] private ToggleGroup _toggleGroup;
    private CanvasCameraHandler _canvasCamera;

    [Header("Misc")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private RawImage _selectedPathLayout;

    private readonly List<SegmentObjectSelection> _objectPositionData = new();
    private SegmentObjectSelection _selectedSegmentObject;
    private GameObject _cachedObjective = null;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void OnEnable()
    {
        _continueButton.onClick.AddListener(OnContinueButtonPressed);
        _continueButton.interactable = false;

        int layoutID = AssessmentManager.Instance.GetSelectedPathLayout();
        _pathPreviewCreator = PathLayoutManager.Instance.GetPathLayout(layoutID);
        _canvasCamera = _pathPreviewCreator.RenderCamera;
        _lineRender = _pathPreviewCreator.DistanceLineController;
        _selectedPathLayout.texture = _pathPreviewCreator.RenderTexture;

        _sliderDistanceValue.onValueChanged.AddListener(OnHorizontalPositionChanged);
        Utils.SetSliderSettings(_sliderDistanceValue, 0, DataManager.Instance.Settings.MovementArea.x, 0f);

        CreateSegmentObjectData();
        OnSelectedObjectChanged(0);
    }

    void OnDisable() 
    {
        _continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        _sliderDistanceValue.onValueChanged.RemoveListener(OnHorizontalPositionChanged);

        foreach (SegmentObjectSelection objectSelection in _objectPositionData)
        {
            objectSelection.SelectedObjectChanged -= OnSelectedObjectChanged;
            Destroy(objectSelection.WorldObject);
            Destroy(objectSelection.gameObject);

        }
        _objectPositionData.Clear();
        _lineRender.ResetPointList();
    }
    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSelectedObjectChanged(int segmentID)
    {
        if (_objectPositionData.Count < 1) return;
        _selectedSegmentObject = _objectPositionData.Find(data => data.SegmentID == segmentID);
        _cachedObjective = _pathPreviewCreator.SpawnedSegments.Find(objective => objective.SegmentID == segmentID).gameObject;

        _selectedSegmentObject.DistanceToObjective = CalculateDistance(_cachedObjective, _selectedSegmentObject.WorldObject);

        _textSegmentID.text = (_selectedSegmentObject.SegmentID + 1).ToString();
        _textDistanceValue.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _textDistanceValueBig.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _objectPreview.texture = ResourceManager.Instance.GetLandmarkObjectRenderTexture(_selectedSegmentObject.ObjectID);

        Vector3 lastValidGlobalPosition = _lineRender.transform.parent.TransformPoint(_selectedSegmentObject.MaxLocalPosition);

        LineRenderer line = _lineRender.GetLineRenderer();
        line.SetPosition(0, _cachedObjective.transform.position);
        line.SetPosition(1, lastValidGlobalPosition);

        float maxDistance = Vector3.Distance(_cachedObjective.transform.position, lastValidGlobalPosition);
        maxDistance = Mathf.Round(maxDistance * 10f) / 10f;
        Utils.SetSliderSettings(_sliderDistanceValue, 0, maxDistance, _selectedSegmentObject.DistanceToObjective);
        _continueButton.interactable = true;
    }

    private void OnHorizontalPositionChanged(float value)
    {
        if (_selectedSegmentObject == null) return;
        if (_cachedObjective == null) return;
        Transform socketObject = _selectedSegmentObject.WorldObject.transform;

        float roundedValue = Mathf.Round(value * 100f) / 100f;

        socketObject.position = _cachedObjective.transform.position + (_selectedSegmentObject.MovementDirection * roundedValue);

        Vector3 newPosition = _canvasCamera.WorldCoordinatesToScreenSpace(socketObject.transform.position);
        _selectedSegmentObject.RectTransform.anchoredPosition = newPosition;
        UpdateSegmentData();
    }

    private void OnContinueButtonPressed()
    {
        AssessmentManager.Instance.ProceedToNextAssessmentStep();
    }
    
    private void OnBackButtonClicked()
    {
        AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void CreateSegmentObjectData()
    {
        foreach (SegmentObjectData segmentData in _pathPreviewCreator.SpawnedSegments)
        {
            if (segmentData.AssignedLandmarkObjectID == -1)
            {
                continue;
            }

            SegmentObjectSelection objectSelection = Instantiate(_selectionObjectPrefab, _positionImage).GetComponent<SegmentObjectSelection>();
            objectSelection.GetComponent<RectTransform>().anchoredPosition = segmentData.CanvasSocketPosition;
            objectSelection.Initialize(segmentData.SegmentID, segmentData.AssignedLandmarkObjectID, segmentData.SocketOffsetAngle, _toggleGroup);
            Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(segmentData.CanvasSocketPosition);
            worldPosition.y = 1;
            objectSelection.InstantiateWorldObject(worldPosition, _pathPreviewCreator.gameObject.transform);
            objectSelection.AutoSelect(segmentData.SegmentID == 0);
            objectSelection.SelectedObjectChanged += OnSelectedObjectChanged;
            _objectPositionData.Add(objectSelection); 

            float radians = Mathf.Deg2Rad * segmentData.SocketOffsetAngle;
            Vector3 direction = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)).normalized;
            objectSelection.MovementDirection = direction;

            objectSelection.MaxLocalPosition = CalculateLastValidPosition(segmentData.transform.localPosition, direction);
        }
    }

    private void UpdateSegmentData()
    {
        _selectedSegmentObject.DistanceToObjective = CalculateDistance(_cachedObjective, _selectedSegmentObject.WorldObject);
        Vector3 realSpawnpoint = _cachedObjective.transform.position + AssessmentManager.Instance.CurrentPath.GetSegmentData(_selectedSegmentObject.SegmentID).RelativeLandmarkPositionToObjective;
        Vector3 objectPosition = _selectedSegmentObject.WorldObject.transform.position;
        objectPosition.y = 0;
        _selectedSegmentObject.DifferenceToRealPosition = Vector3.Distance(realSpawnpoint, objectPosition);

        AssessmentManager.Instance.SetSegmentLandmarkObjectDistance(_selectedSegmentObject.SegmentID, _selectedSegmentObject.DistanceToObjective,
            _selectedSegmentObject.DifferenceToRealPosition);

        _textDistanceValue.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";
        _textDistanceValueBig.text = _selectedSegmentObject.DistanceToObjective.ToString("F2", CultureInfo.InvariantCulture) + " m";

        _continueButton.interactable = VerifyPositionValues();
    }


    public float CalculateDistance(GameObject from, GameObject to)
    {
        Vector3 fromPos = from.transform.position;
        fromPos.y = 0f; 
        Vector3 ToPos = to.transform.position;
        ToPos.y = 0f; 
        return Vector3.Distance(fromPos, ToPos);
    }

    private bool VerifyPositionValues()
    {
        foreach (var positionData in _objectPositionData)
        {
            if (positionData.DistanceToObjective <= 0)
                return false;
        }
        return true;
    }

    public void ResetPanelData()
    {
    }

    /// <summary>
    /// Calculates the last valid positon of an object within the movement boundary.
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="direction"></param>
    /// <returns>Last valid position in local space.</returns>
    private Vector3 CalculateLastValidPosition(Vector3 currentPosition, Vector3 direction)
    {
        float halfWidth = DataManager.Instance.Settings.MovementArea.x / 2;
        float halfHeight = DataManager.Instance.Settings.MovementArea.y / 2;

        while (true)
        {
            Vector3 nextPosition = currentPosition + direction * 0.1f;
            if (nextPosition.z < - halfWidth  || nextPosition.z > halfWidth || 
                nextPosition.x < - halfHeight || nextPosition.x > halfHeight )
            {
                Debug.DrawLine(currentPosition, currentPosition, Color.red, 1f);
                return currentPosition;
            }
            currentPosition = nextPosition;
        }
    }
}
