using System;
using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep {IDLE, PATHSELECTION, OBJECTIVEDISTANCE, OBJECTSELECTION,
                            OBJECTDISTANCE, OBJECTIVEOBJECTSELECTION, NEXTPATH, COMPLETED,
                            OBJECTASSIGN, OBJECTPOSITION, SEGMENTDISTANCE}

public class AssessmentManager : MonoBehaviour
{
    public static AssessmentManager Instance { get; private set; }
    private UnityEvent<AssessmentStep> _assessmentStepChanged = new();
    private AssessmentStep _assessmentStep;
    public AssessmentStep AssessmentStep
    {
        get => _assessmentStep;
        set
        {
            _assessmentStep = value;
            _assessmentStepChanged?.Invoke(value);
        }
    }
    [SerializeField] private GameObject _pathSelectionPanel;
    [SerializeField] private GameObject _objectiveDistancePanel;
    [SerializeField] private GameObject _objectiveObjectPanel;
    [SerializeField] private GameObject _objectSelectionPanel;
    [SerializeField] private GameObject _objectDistancePanel;
    [SerializeField] private GameObject _objectAssignPanel;
    [SerializeField] private GameObject _objectPositionPanel;
    [SerializeField] private GameObject _segmentDistancePanel;
    [SerializeField] private GameObject _nextPathPanel;
    private GameObject _activePanel;
    private int _currentAssessmentStep;

    [Header("Assessment")]
    public PathData CurrentPath;
    public PathAssessment CurrentPathAssessment;
    private AssessmentData _assessment;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _assessmentStepChanged.AddListener(OnEvaluationStepChanged);
        _assessmentStep = AssessmentStep.IDLE;
        CurrentPathAssessment = null;
        _assessment = null;
    }

    private void OnDestroy()
    {
        _assessmentStepChanged.RemoveListener(OnEvaluationStepChanged);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEvaluationStepChanged(AssessmentStep newStep)
    {
        if (_activePanel != null)
            _activePanel.SetActive(false);
        switch (newStep)
        {
            case AssessmentStep.IDLE:
                _activePanel = null;
                break;
            case AssessmentStep.PATHSELECTION:
                _activePanel = _pathSelectionPanel;
                break;
            case AssessmentStep.OBJECTIVEDISTANCE:
                _activePanel = _objectiveDistancePanel;
                break;
            case AssessmentStep.OBJECTIVEOBJECTSELECTION:
                _activePanel = _objectiveObjectPanel;
                break;
            case AssessmentStep.OBJECTSELECTION:
                _activePanel = _objectSelectionPanel;
                break;
            case AssessmentStep.OBJECTDISTANCE:
                _activePanel = _objectDistancePanel;
                break;
            case AssessmentStep.OBJECTASSIGN:
                _activePanel = _objectAssignPanel;
                break;
            case AssessmentStep.OBJECTPOSITION:
                _activePanel = _objectPositionPanel;
                break;
            case AssessmentStep.SEGMENTDISTANCE:
                _activePanel = _segmentDistancePanel;
                break;
            case AssessmentStep.NEXTPATH:
                _activePanel = _nextPathPanel;
                break;
            case AssessmentStep.COMPLETED:
                DataManager.Instance.SaveAssessmentData(_assessment);
                CurrentPath = null;
                CurrentPathAssessment = null;
                AssessmentStep = AssessmentStep.IDLE;
                ExperimentManager.Instance.PathAssessmentCompleted();
                break;
        }
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void CreateNewAssessment(int experimentID, PathData pathData, FloorType floorType)
    {
        CurrentPath = pathData;
        CurrentPathAssessment = new PathAssessment(pathData);
        Debug.Log($"Assessment (Participant {experimentID}) (Path: {pathData.name}).");
        _assessment ??= new(experimentID, DateTime.Now);
        _assessment.AddPath(pathData, floorType, ExperimentManager.Instance.Timer.GetElapsedTimeFormated());

        _currentAssessmentStep = 0;
    }

    public void StartAssessment()
    {
        ProceedToNextAssessmentStep();
    }

    public void ProceedToNextAssessmentStep()
    {
        switch (CurrentPath.Type)
        {
            case PathType.DEFAULT:
                switch (_currentAssessmentStep)
                {
                    case 0:
                        AssessmentStep = AssessmentStep.PATHSELECTION;
                        break;
                    case 1:
                        AssessmentStep = AssessmentStep.SEGMENTDISTANCE;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.NEXTPATH;
                        break;
                    case 3:
                        AssessmentStep = AssessmentStep.COMPLETED;
                        break;
                }
                break;
            case PathType.EXTENDED:
                switch (_currentAssessmentStep)
                {
                    case 0:
                        AssessmentStep = AssessmentStep.PATHSELECTION;
                        break;
                    case 1:
                        AssessmentStep = AssessmentStep.SEGMENTDISTANCE;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.OBJECTASSIGN;
                        break;
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTPOSITION;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.COMPLETED;
                        break;
                }
                break;
        }
        Debug.Log($"AssessmentManager :: Proceed to step: {_currentAssessmentStep}");
        _currentAssessmentStep++;
    }

    public void GoToPreviousAssessmentStep()
    {
        switch (CurrentPath.Type)
        {
            case PathType.DEFAULT:
                switch (_currentAssessmentStep)
                {
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTIVEDISTANCE;
                        _currentAssessmentStep = 2;
                        break;
                }
                break;
            case PathType.EXTENDED:
                switch (_currentAssessmentStep)
                {
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTIVEOBJECTSELECTION;
                        _currentAssessmentStep = 2;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.OBJECTSELECTION;
                        _currentAssessmentStep = 3;
                        break;
                    case 5:
                        AssessmentStep = AssessmentStep.OBJECTDISTANCE;
                        _currentAssessmentStep = 4;
                        break;
                }
                break;
        }
        Debug.Log($"AssessmentManager :: Go back to step: {_currentAssessmentStep}");
    }

    /// <summary>
    /// Saves the selected path layout.
    /// </summary>
    /// <param name="selectedPathLayoutID"></param>
    public void SetSelectedPathLayout(int selectedPathLayoutID)
    {
        CurrentPathAssessment.SelectedPathLayoutID = selectedPathLayoutID;
        PathAssessmentData pathAssessment = _assessment.GetPath(CurrentPath.PathID);
        pathAssessment.CorrentPathLayoutSelected = CurrentPathAssessment.EvaluateSelectedPathImage();
        Debug.Log($"AssessmentManager :: SetSelectedPathImage : Correct path -> {pathAssessment.CorrentPathLayoutSelected}");
    }

    /// <summary>
    /// Save the selected distance from current segment to previous. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetSegmentObjectiveDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceToPreviousSegment = distanceValue;
        }

        SegmentAssessmentData segmentAssessmentData = _assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.SelectedDistanceToPreviousSegment = distanceValue;
        segmentAssessmentData.ActualDistanceToPreviousSegment = CurrentPath.GetSegmentData(segmentID).DistanceFromPreviousSegment;
        segmentAssessmentData.SegmentDistanceError = Math.Abs(segmentAssessmentData.ActualDistanceToPreviousSegment - distanceValue);
    }

    /// <summary>
    /// Saves the selected distance of the object to the objective for the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceToObjective"></param>
    /// <param name="differenceToRealObject"></param>
    public void SetSegmentLandmarkObjectDistance(int segmentID, float distanceToObjective, float differenceToRealObject)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceOfObjectToObjective = distanceToObjective;
            segmentAssessment.SelectedDistanceOfObjectToRealObject = differenceToRealObject;
        }

        SegmentAssessmentData segmentAssessmentData = _assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.LandmarkDifferenceToRealObject = differenceToRealObject;
        segmentAssessmentData.SelectedLandmarkDistanceToObjective = distanceToObjective;
        segmentAssessmentData.ActualLandmarkDistanceToObjective = CurrentPath.GetSegmentData(segmentID).LandmarkObjectDistanceToObjective;
    }

    /// <summary>
    /// Assigns the selected objective object to the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignSegmentHoverObject(int segmentID, int objectID)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedObjectiveObjectID = objectID;
            SegmentAssessmentData segmentAssessmentData = _assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectHoverObjectSelected = segmentAssessment.AssessObjectiveObject();
            Debug.Log($"AssessmentManager :: Hover Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectHoverObjectSelected}.");
        }
    }

    /// <summary>
    /// Assigns the selected landmark object to the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignSegmentLandmarkObject(int segmentID, int objectID)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedSegmentObjectID = objectID;
            SegmentAssessmentData segmentAssessmentData = _assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectLandmarkObjectSelected = segmentAssessment.AssessSegmentObject();
            Debug.Log($"AssessmentManager :: Landmark Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectLandmarkObjectSelected}.");
        }
    }

    public void FinishAssessment()
    {
        _assessment.Completed = true;
        DataManager.Instance.SaveAssessmentData(_assessment);
        DataManager.Instance.Settings.CompletedExperiments++;
        _assessment = null;
    }

    [Obsolete("Function might be removed in the future.")]
    public void ResetPanelData()
    {
        _objectiveDistancePanel.GetComponent<UIObjectiveDistanceSelection>().ResetPanelData();
        _objectiveObjectPanel.GetComponent<UIObjectiveObjectSelection>().ResetPanelData();
        _objectSelectionPanel.GetComponent<UISegmentObjectSelection>().ResetPanelData();
        _objectDistancePanel.GetComponent<UISegmentObjectPosition>().ResetPanelData();
    }

    public AssessmentData GetAssessment()
    {
        return _assessment;
    }

    public void ResetAssessment()
    {
        CurrentPath = null;
        CurrentPathAssessment = null;
        AssessmentStep = AssessmentStep.IDLE;
    }

}
