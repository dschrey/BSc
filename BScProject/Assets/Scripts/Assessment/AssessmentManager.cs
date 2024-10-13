using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep {IDLE, PATHSELECTION, OBJECTIVEDISTANCE, OBJECTSELECTION, OBJECTDISTANCE, OBJECTIVEOBJECTSELECTION, COMPLETED }

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
    private GameObject _activePanel;
    private int _currentAssessmentStep;

    [Header("Assessment Results")]
    public PathData CurrentPath;

    public PathAssessment PathAssessment;
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
                ExperimentManager.Instance._XROrigin.LookAt(_pathSelectionPanel.transform);
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
            case AssessmentStep.COMPLETED:
                _activePanel = null;
                ExperimentManager.Instance.PathAssessmentCompleted();
                DataManager.Instance.SaveAssessmentData(Assessment);
                CurrentPath = null;
                PathAssessment = null;
                _assessmentStep = AssessmentStep.IDLE;
				break;
		}
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void StartPathAssessment(PathData currentPath)
    {
        CurrentPath = currentPath;
        PathAssessment = new PathAssessment(currentPath);
        Debug.Log($"Started assessment on path {currentPath.PathID} at {System.DateTime.Now}");
        Assessment ??= new(ExperimentManager.Instance.ExperimentSettings.CompletedAssessments, System.DateTime.Now);
        Assessment.AddPath(currentPath);
        
        _currentAssessmentStep = 0;
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
                        AssessmentStep = AssessmentStep.OBJECTIVEOBJECTSELECTION;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.OBJECTSELECTION;
                        break;
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTDISTANCE;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.COMPLETED;
                        break;
                }
                break;
        }
        _currentAssessmentStep++;
        Debug.Log($"Current assessment step {_currentAssessmentStep}");
    }

    /// <summary>
    /// Sets the selected path Image.
    /// </summary>
    /// <param name="seletedPathImage"></param>
    public void SetSelectedPathImage(Sprite seletedPathImage)
    {
        PathAssessment.SelectedPathSprite = seletedPathImage;
        PathAssessmentData pathAssessment = Assessment.GetPath(CurrentPath.PathID);
        pathAssessment.CorrentPathImageSelected = PathAssessment.EvaluateSelectedPathImage();
    }

    /// <summary>
    /// Assigns the selected objective distance to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetPathSegmentObjectiveDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceToPreviousSegment = distanceValue;
        }

        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.SelectedDistanceToPreviousSegment = distanceValue;
        segmentAssessmentData.CalculatedDistanceToPreviousSegment = CurrentPath.GetSegmentData(segmentID).DistanceToPreviousSegment;
    }

    /// <summary>
    /// Assigns the selected distance of the object distance to the objective for the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetPathSegmentObjectDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceOfObjectToObjective = distanceValue;
            
        }

        
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.SelectedObjectDistanceToObjective = distanceValue;
        segmentAssessmentData.CalculatedObjectDistanceToObjective = CurrentPath.GetSegmentData(segmentID).ObjectDistanceToObjective;
    }
     
    /// <summary>
    /// Assigns the selected objective object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="selectedRenderTexture"></param>
    public void AssignPathSegmentObjectiveObject(int segmentID, RenderTexture selectedRenderTexture)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedObjectiveObjectRenderTexture = selectedRenderTexture;
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectObjectiveObjectSelected = segmentAssessment.EvaluateObjectiveObjectAssignment();
        }


    }
     
    /// <summary>
    /// Assigns the selected segment object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="selectedRenderTexture"></param>
    public void AssignPathSegmentObject(int segmentID, RenderTexture selectedRenderTexture)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedSegmentObjectRenderTexture = selectedRenderTexture;
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectSegmentObjectSelected = segmentAssessment.EvaluateSegmentObjectAssignment();
        }
    }

}
