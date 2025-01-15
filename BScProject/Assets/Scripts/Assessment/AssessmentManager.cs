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
        Assessment ??= new(ExperimentManager.Instance.ExperimentSettings.CompletedAssessments, DateTime.Now);
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
                        AssessmentStep = AssessmentStep.NEXTPATH;
                        break;
                    case 5:
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
        PathAssessmentData pathAssessment = Assessment.GetPath(CurrentPath.PathID);
        pathAssessment.CorrentPathImageSelected = CurrentPathAssessment.EvaluateSelectedPathImage();
        Debug.Log($"AssessmentManager :: SetSelectedPathImage : Correct path -> {pathAssessment.CorrentPathImageSelected}");
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

        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.SelectedDistanceToPreviousSegment = distanceValue;
        segmentAssessmentData.ActualDistanceToPreviousSegment = CurrentPath.GetSegmentData(segmentID).DistanceFromPreviousSegment;
        segmentAssessmentData.SegmentDistanceDifference = Math.Abs(segmentAssessmentData.ActualDistanceToPreviousSegment - distanceValue);
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

        SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
        segmentAssessmentData.ObjectDifferenceToRealObject = differenceToRealObject;
        segmentAssessmentData.SelectedObjectDistanceToObjective = distanceToObjective;
        segmentAssessmentData.CalculatedObjectDistanceToObjective = CurrentPath.GetSegmentData(segmentID).LandmarkObjectDistanceToObjective;
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
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectObjectiveObjectSelected = segmentAssessment.AssessObjectiveObject();
            Debug.Log($"AssessmentManager :: Hover Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectObjectiveObjectSelected}.");
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
            SegmentAssessmentData segmentAssessmentData = Assessment.GetPath(CurrentPath.PathID).GetSegment(segmentID);
            segmentAssessmentData.CorrectSegmentObjectSelected = segmentAssessment.AssessSegmentObject();
            Debug.Log($"AssessmentManager :: Landmark Object -> Segment {segmentID} - Correct: {segmentAssessmentData.CorrectSegmentObjectSelected}.");
        }
    }

    public void FinishAssessment()
    {
        Assessment.Completed = true;
        DataManager.Instance.SaveAssessmentData(Assessment);
        ExperimentManager.Instance.ExperimentSettings.CompletedAssessments++; 
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


    // -------------------------------------------- Start scene integration -------------------------------------------------------------

    public void CreateNewAssessment(int ID, string participant, PathData pathData)
    {
        CurrentPath = pathData;
        // CurrentPathAssessment = new PathAssessment(pathData);
        Debug.Log($"AssessmentManager :: CreateNewAssessment() : New assessment (ID: {ID}).");
        Assessment ??= new(ID, participant, DateTime.Now);
        Assessment.AddPath(pathData);
        
        _currentAssessmentStep = 0;
        // ProceedToNextAssessmentStep();
        _assessmentStep = AssessmentStep.IDLE;

        FindObjectOfType<TeleportFade>().FadeOutScene("ExperimentScene");

    }

}
