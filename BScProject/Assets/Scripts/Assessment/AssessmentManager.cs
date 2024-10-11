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
    public PathAssessment PathAssessmentData;


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
                // TODO Save data;
                CurrentPath = null;
                PathAssessmentData = null;
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
        PathAssessmentData = new PathAssessment(currentPath);
        
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
        PathAssessmentData.SelectedPathSprite = seletedPathImage;
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

    /// <summary>
    /// Assigns the selected distance of the object distance to the objective for the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetPathSegmentObjectDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceOfObjectToObjective = distanceValue;
        }
    }
     
    /// <summary>
    /// Assigns the selected objective object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="selectedObjectRenderTexture"></param>
    public void AssignPathSegmentObjectiveObject(int segmentID, RenderTexture selectedObjectRenderTexture)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedObjectiveObjectRenderTexture = selectedObjectRenderTexture;
        }
    }
     
    /// <summary>
    /// Assigns the selected segment object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="selectedObjectRenderTexture"></param>
    public void AssignPathSegmentObject(int segmentID, RenderTexture selectedObjectRenderTexture)
    {
        foreach (PathSegmentAssessment segmentAssessment in PathAssessmentData.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedSegmentObjectRenderTexture = selectedObjectRenderTexture;
        }
    }

}
