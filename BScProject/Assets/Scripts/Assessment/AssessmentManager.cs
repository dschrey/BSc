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
    public AssessmentData Assessment;

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
        Assessment = null;
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
                DataManager.Instance.SaveAssessmentData(Assessment);
                _activePanel = null;
                CurrentPath = null;
                _assessmentStep = AssessmentStep.IDLE;
                ExperimentManager.Instance.PathAssessmentCompleted();
                return;
        }
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void PrepareAssessment(int experimentID, PathData pathData, FloorType floorType, AssessmentData assessment)
    {
        CurrentPath = pathData;
        Debug.Log($"Assessment (Participant {experimentID}) (Path: {pathData.PathID} - {pathData.PathName}).");
        Assessment = assessment;
        Assessment.AddPath(pathData, floorType);

        _currentAssessmentStep = 0;
    }

    public void StartAssessment()
    {
        Assessment.SetPathTime(CurrentPath.PathName, ExperimentManager.Instance.Timer.GetElapsedTimeFormated());
        ProceedToNextAssessmentStep();
    }

    public void ProceedToNextAssessmentStep()
    {
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
                AssessmentStep = AssessmentStep.COMPLETED;
                return;
        }
        Debug.Log($"AssessmentManager :: Proceed to step: {_currentAssessmentStep}");
        _currentAssessmentStep++;
    }

    public void GoToPreviousAssessmentStep()
    {
        switch (_currentAssessmentStep)
        {
            case 2:
                AssessmentStep = AssessmentStep.PATHSELECTION;
                _currentAssessmentStep = 1;
                break;
            case 3:
                AssessmentStep = AssessmentStep.SEGMENTDISTANCE;
                _currentAssessmentStep = 2;
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
        PathAssessmentData pathAssessment = Assessment.GetPath(CurrentPath.PathName);
        if (pathAssessment == null)
        {
            Debug.LogWarning($"PathAssessment of path ({CurrentPath.PathID} - {CurrentPath.PathName}) is null;");
            return;

        }
        pathAssessment.SetPathLayout(selectedPathLayoutID);
    }

    /// <summary>
    /// Save the selected distance from current segment to previous. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetSegmentObjectiveDistance(int segmentID, float distanceValue)
    {
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathName).GetSegment(segmentID);
        if (segmentAssessmentData == null)
        {
            Debug.LogWarning($"segmentAssessmentData of segment {segmentID} is null");
        }
        segmentAssessmentData.SetObjectiveDistance(distanceValue);
    }

    /// <summary>
    /// Saves the selected distance of the object to the objective for the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceToObjective"></param>
    /// <param name="differenceToRealObject"></param>
    public void SetSegmentLandmarkObjectDistance(int segmentID, float distanceToObjective, float differenceToRealObject)
    {
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathName).GetSegment(segmentID);
        segmentAssessmentData.SetLandmarkDistances(distanceToObjective, differenceToRealObject);
    }

    /// <summary>
    /// Assigns the selected objective object to the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignSegmentHoverObject(int segmentID, int objectID)
    {
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathName).GetSegment(segmentID);
        segmentAssessmentData.SetHoverObject(objectID);
    }

    /// <summary>
    /// Assigns the selected landmark object to the specified segment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignSegmentLandmarkObject(int segmentID, int objectID)
    {
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathName).GetSegment(segmentID);
        segmentAssessmentData.SetLandmarkObject(objectID);
    }

    [Obsolete]
    public void FinishAssessment()
    {
        Assessment.Completed = true;
        DataManager.Instance.SaveAssessmentData(Assessment);
        DataManager.Instance.Settings.CompletedExperiments++;
        Assessment = null;
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
        return Assessment;
    }

    public void ResetAssessment()
    {
        CurrentPath = null;
        AssessmentStep = AssessmentStep.IDLE;
    }

    public int GetSelectedLayoutOfCurrentPath()
    {
        return Assessment.GetPath(CurrentPath.PathName).SelectedPathLayout;
    }

}
