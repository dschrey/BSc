using System;
using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep {IDLE, PATHSELECTION, OBJECTIVEDISTANCE, OBJECTSELECTION, OBJECTDISTANCE, OBJECTIVEOBJECTSELECTION, NEXTPATH, COMPLETED }

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
    [SerializeField] private GameObject _nextPathPanel;
    private GameObject _activePanel;
    private int _currentAssessmentStep;

    [Header("Assessment Results")]
    public PathData CurrentPath;

    public PathAssessment CurrentPathAssessment;
    private AssessmentData Assessment;


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
        Assessment = null;
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
            case AssessmentStep.NEXTPATH:
                _activePanel = _nextPathPanel;
				break;
            case AssessmentStep.COMPLETED:
                ExperimentManager.Instance.PathAssessmentCompleted();
                DataManager.Instance.SaveAssessmentData(Assessment);
                CurrentPath = null;
                CurrentPathAssessment = null;
                AssessmentStep = AssessmentStep.IDLE;
				break;
		}
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void StartPathAssessment(PathData currentPath)
    {
        CurrentPath = currentPath;
        CurrentPathAssessment = new PathAssessment(currentPath);
        Debug.Log($"AssessmentManager :: StartPathAssessment() : Started assessment on path (ID: {currentPath.PathID}).");
        Assessment ??= new(ExperimentManager.Instance.ExperimentSettings.CompletedAssessments, System.DateTime.Now);
        Assessment.AddPath(currentPath, (int)Math.Floor(ExperimentManager.Instance.Timer.GetElapsedTime()));
        
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
                        AssessmentStep = AssessmentStep.OBJECTIVEOBJECTSELECTION;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.OBJECTSELECTION;
                        break;
                    case 3:
                        AssessmentStep = AssessmentStep.OBJECTDISTANCE;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.NEXTPATH;
                        break;
                    case 5:
                        AssessmentStep = AssessmentStep.COMPLETED;
                        break;
                }
                break;
        }
        Debug.Log($"AssessmentManager :: ProceedToNextAssessmentStep() : Assessment step: {_currentAssessmentStep}");
        _currentAssessmentStep++;
    }

    /// <summary>
    /// Sets the selected path Image.
    /// </summary>
    /// <param name="seletedPathImage"></param>
    public void SetSelectedPathImage(Sprite seletedPathImage)
    {
        CurrentPathAssessment.SelectedPathSprite = seletedPathImage;
        PathAssessmentData pathAssessment = Assessment.GetPath(CurrentPath.PathID);
        pathAssessment.CorrentPathImageSelected = CurrentPathAssessment.EvaluateSelectedPathImage();
        
        Debug.Log($"AssessmentManager :: SetSelectedPathImage : Correct path -> {pathAssessment.CorrentPathImageSelected}");
    }

    /// <summary>
    /// Assigns the selected objective distance to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="distanceValue"></param>
    public void SetPathSegmentObjectiveDistance(int segmentID, float distanceValue)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
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
    /// <param name="distanceToObjective"></param>
    /// <param name="distanceToRealObject"></param>
    public void SetPathSegmentObjectDistance(int segmentID, float distanceToObjective, float distanceToRealObject)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedDistanceOfObjectToObjective = distanceToObjective;
            segmentAssessment.SelectedDistanceOfObjectToRealObject = distanceToRealObject;
        }

        
        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.ObjectDistanceToRealObject = distanceToRealObject;
        segmentAssessmentData.SelectedObjectDistanceToObjective = distanceToObjective;
        segmentAssessmentData.CalculatedObjectDistanceToObjective = CurrentPath.GetSegmentData(segmentID).ObjectDistanceToObjective;
    }
     
    /// <summary>
    /// Assigns the selected objective object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignPathSegmentObjectiveObject(int segmentID, int objectID)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedObjectiveObjectID = objectID;
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            // segmentAssessmentData.CorrectObjectiveObjectSelected = segmentAssessment.EvaluateObjectiveObjectAssignment();
            segmentAssessmentData.CorrectObjectiveObjectSelected = segmentAssessment.AssessObjectiveObject();
            Debug.Log($"AssessmentManager :: Objective Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectObjectiveObjectSelected}.");
        }


    }
     
    /// <summary>
    /// Assigns the selected segment object to the specified segment for assessment. 
    /// </summary>
    /// <param name="segmentID"></param>
    /// <param name="objectID"></param>
    public void AssignPathSegmentObject(int segmentID, int objectID)
    {
        foreach (PathSegmentAssessment segmentAssessment in CurrentPathAssessment.PathSegmentAssessments)
        {
            if (segmentAssessment.GetSegmentID() != segmentID)
                continue;

            segmentAssessment.SelectedSegmentObjectID = objectID;
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectSegmentObjectSelected = segmentAssessment.AssessSegmentObject();
            Debug.Log($"AssessmentManager :: Segment Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectSegmentObjectSelected}.");
        }
    }

    public void FinishAssessment()
    {
        Assessment.Completed = true;
        DataManager.Instance.SaveAssessmentData(Assessment);
        ExperimentManager.Instance.ExperimentSettings.CompletedAssessments++; 
        Assessment = null;
    }

}
