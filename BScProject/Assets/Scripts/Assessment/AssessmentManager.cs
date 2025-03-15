using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep {Idle, PathSelection, SegmentDistance, HoverObjectSelection, LandmarkSelection, Completed}

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
    [SerializeField] private GameObject _objectAssignPanel;
    [SerializeField] private GameObject _objectPositionPanel;
    [SerializeField] private GameObject _segmentDistancePanel;
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
        _assessmentStep = AssessmentStep.Idle;
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
            case AssessmentStep.Idle:
                _activePanel = null;
                break;
            case AssessmentStep.PathSelection:
                _activePanel = _pathSelectionPanel;
                break;
            case AssessmentStep.HoverObjectSelection:
                _activePanel = _objectAssignPanel;
                break;
            case AssessmentStep.LandmarkSelection:
                _activePanel = _objectPositionPanel;
                break;
            case AssessmentStep.SegmentDistance:
                _activePanel = _segmentDistancePanel;
                break;
            case AssessmentStep.Completed:
                DataManager.Instance.SaveAssessmentData(Assessment);
                _activePanel = null;
                CurrentPath = null;
                _assessmentStep = AssessmentStep.Idle;
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

    public void StartAssessment(int numHints)
    {
        Assessment.AddPathInformation(CurrentPath.PathName, ExperimentManager.Instance.Timer.GetElapsedTimeFormated(), numHints);
        ProceedToNextAssessmentStep();
    }

    public void ProceedToNextAssessmentStep()
    {
        switch (_currentAssessmentStep)
        {
            case 0:
                AssessmentStep = AssessmentStep.PathSelection;
                break;
            case 1:
                AssessmentStep = AssessmentStep.SegmentDistance;
                break;
            case 2:
                AssessmentStep = AssessmentStep.HoverObjectSelection;
                break;
            case 3:
                AssessmentStep = AssessmentStep.Completed;
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
                AssessmentStep = AssessmentStep.PathSelection;
                _currentAssessmentStep = 1;
                break;
            case 3:
                AssessmentStep = AssessmentStep.SegmentDistance;
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

    public AssessmentData GetAssessment()
    {
        return Assessment;
    }

    public void ResetAssessment()
    {
        CurrentPath = null;
        AssessmentStep = AssessmentStep.Idle;
    }

    public int GetSelectedPathLayout()
    {
        return Assessment.GetPath(CurrentPath.PathName).SelectedPathLayout;
    }

}
