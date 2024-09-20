using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep {IDLE, PATHSELECTION, OBJECTIVEDISTANCE, OBSTACLELOCATION, OBSTACLEDISTANCE, OBJECTIVEOBJECT, COMPLETED }

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
    [SerializeField] private GameObject _itemDistancePanel;
    [SerializeField] private GameObject _obstacleLocationPanel;
    [SerializeField] private GameObject _obstacleDistancePanel;
    [SerializeField] private GameObject _objectiveObjectPanel;
    private GameObject _activePanel;
    private int _currentAssessmentStep;

    [Header("Assessment Results")]

    // TODO AssessmentResults handling
    private PathData _currentPath;
    public PathData SelectedPath;
    public PathAssessmentData PathAssessmentData;


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
                _activePanel = _itemDistancePanel;
				break;
            case AssessmentStep.OBSTACLELOCATION:
                _activePanel = _obstacleLocationPanel;
				break;
            case AssessmentStep.OBSTACLEDISTANCE:
                _activePanel = _obstacleDistancePanel;
				break;
            case AssessmentStep.COMPLETED:
                _activePanel = null;
				break;
		}
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void StartPathAssessment(PathData currentPath)
    {
        _currentPath = currentPath;
        PathAssessmentData = new PathAssessmentData(currentPath);
        
        SelectedPath = null;
        _currentAssessmentStep = 0;
        ProceedToNextAssessmentStep();
    }

    public void ProceedToNextAssessmentStep()
    {
        switch (_currentPath.Type)
        {
            case PathType.DEFAULT:
                switch (_currentAssessmentStep)
                {
                    case 0:
                        AssessmentStep = AssessmentStep.PATHSELECTION;
                        break;
                    case 1:
                        PathAssessmentData.SelectedPath = SelectedPath;
                        AssessmentStep = AssessmentStep.OBJECTIVEDISTANCE;
                        break;
                    case 2:
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
                        PathAssessmentData.SelectedPath = SelectedPath;
                        AssessmentStep = AssessmentStep.OBSTACLEDISTANCE;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.OBSTACLELOCATION;
                        break;
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTIVEOBJECT;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.COMPLETED;
                        break;
                }
                break;
        }
        _currentAssessmentStep++;
    }

    /// <summary>
    /// Assigns the selected objective distance to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetPathSegmentDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceToPreviousSegment = distanceValue;
        }
    }

    public void AssignPathSegmentObject(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceToPreviousSegment = distanceValue;
        }
    }
     
    /// <summary>
    /// Assigns the selected objective object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="selectedObjectSprite"></param>
    public void AssignPathSegmentObjectiveObject(int segmentID, Sprite selectedObjectSprite)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedObjectiveObjectSprite = selectedObjectSprite;
        }
    }

}
