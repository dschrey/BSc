using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AssessmentStep
{
    Idle,
    PathLayout,
    SegmentDistance,
    HoverObjectSelection,
    LandmarkSelection,
    Completed
}


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
    [SerializeField]  private GameObject _pathSelectionPanel;
    [SerializeField]  private GameObject _segmentDistancePanel;
    [SerializeField]  private GameObject _hoverObjectSelectionPanel;
    [SerializeField]  private GameObject _landmarkSelectionPanel;

    private GameObject _activePanel;
    private int _currentAssessmentStep;

    [Header("Assessment")]
    public PathData CurrentPath;
    public AssessmentData Assessment;
    private PathAssessmentData _currentPathAssessmentData => Assessment.GetPath(CurrentPath.PathName);

    private float _navigationTimeSec = -1;
    private float _backtrackTimeSec = -1;
    private float _absDistanceOffsetToOrigin = -1;
    private float _actualDistancetoOrigin = -1;
    private float _selectedDistancetoOrigin = -1;
    private float _bearingErrorToOrigin = -1;
    private float _absBearingErrorToOrigin = -1;
    private int _selectedPathLayout = -1;
    public List<TrialSegmentData> _trialSegmentData = new();

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
            case AssessmentStep.PathLayout:
                StudyManager.Instance.AssessmentTimer.StartTimer();
                _activePanel = _pathSelectionPanel;
                AudioManager.Instance.PlayAudio(SoundType.InstructionPathLayoutSelection);
                break;
            case AssessmentStep.HoverObjectSelection:
                _activePanel = _hoverObjectSelectionPanel;
                break;
            case AssessmentStep.LandmarkSelection:
                _activePanel = _landmarkSelectionPanel;
                AudioManager.Instance.PlayAudio(SoundType.InstructionLandmarkSelection);
                if (StudyManager.Instance.StudyData.PrimaryHand == PrimaryHand.Right)
                {
                    AudioManager.Instance.PlayAudio(SoundType.InstructionLandmarkPlacementRight);
                }
                else
                {
                    AudioManager.Instance.PlayAudio(SoundType.InstructionLandmarkPlacementLeft);
                }
                break;
            case AssessmentStep.SegmentDistance:
                _activePanel = _segmentDistancePanel;
                AudioManager.Instance.PlayAudio(SoundType.InstructionPathBuilding);
                break;
            case AssessmentStep.Completed:
                StudyManager.Instance.SetTrialData(_navigationTimeSec, _actualDistancetoOrigin, _selectedDistancetoOrigin,
                    _absDistanceOffsetToOrigin, _bearingErrorToOrigin, _absBearingErrorToOrigin,
                    _backtrackTimeSec, _selectedPathLayout, _trialSegmentData);
                _activePanel = null;
                CurrentPath = null;
                _assessmentStep = AssessmentStep.Idle;
                ExperimentManager.Instance.ExperimentState = ExperimentState.Finished;
                return;
        }
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void PrepareTrialAssessment(PathData pathData)
    {
        foreach (PathSegmentData pathSegment in pathData.SegmentsData)
        {
            string landmarkObjectName = ResourceManager.Instance.GetLandmarkObject(pathSegment.LandmarkObjectID).name.Replace("(Clone)", "").Trim();
            _trialSegmentData.Add(new(pathSegment.SegmentID, pathSegment.DistanceToPreviousSegment,
                pathSegment.AngleFromPreviousSegment, pathSegment.AngleToLandmark, pathSegment.LandmarkDistanceToSegment,
                landmarkObjectName));
        }
        CurrentPath = pathData;
    }

    public void StartAssessment() =>  ProceedToNextAssessmentStep();

    public void ProceedToNextAssessmentStep()
    {
        switch (CurrentPath.PathDifficulty)
        {
            case PathDifficulty.Easy:
                switch (_currentAssessmentStep)
                {
                    case 0:
                        AssessmentStep = AssessmentStep.PathLayout;
                        break;
                    case 1:
                        AssessmentStep = AssessmentStep.SegmentDistance;
                        break;
                    case 2:
                        AssessmentStep = AssessmentStep.Completed;
                        return;
                }
                break;
            case PathDifficulty.Hard:
                switch (_currentAssessmentStep)
                {
                    case 0:
                        AssessmentStep = AssessmentStep.PathLayout;
                        break;
                    case 1:
                        AssessmentStep = AssessmentStep.SegmentDistance;
                        break;
                    case 2:
                        if ((CurrentPath.PathObjects & PathObjects.Hover) == 0)
                        {
                            _currentAssessmentStep++;
                            ProceedToNextAssessmentStep();
                            return;
                        }
                        AssessmentStep = AssessmentStep.HoverObjectSelection;
                        break;
                    case 3:
                        if ((CurrentPath.PathObjects & PathObjects.Landmarks) == 0)
                        {
                            _currentAssessmentStep++;
                            ProceedToNextAssessmentStep();
                            return;
                        }
                        AssessmentStep = AssessmentStep.LandmarkSelection;
                        break;
                    case 4:
                        AssessmentStep = AssessmentStep.Completed;
                        return;
                }
                break;
        }
       
        Debug.Log($"AssessmentManager :: Proceed to step: {_currentAssessmentStep}");
        _currentAssessmentStep++;
    }

    public void GoToPreviousAssessmentStep()
    {
        switch (_currentAssessmentStep)
        {
            case 1:
                _currentAssessmentStep = 0;
                AssessmentStep = AssessmentStep.PathLayout;
                break;
            case 2:
                _currentAssessmentStep = 1;
                AssessmentStep = AssessmentStep.SegmentDistance;
                break;
            case 3:
                _currentAssessmentStep = 2;
                if ((CurrentPath.PathObjects & PathObjects.Landmarks) == 0)
                {
                    GoToPreviousAssessmentStep();
                    return;
                }
                AssessmentStep = AssessmentStep.HoverObjectSelection;
                break;
            case 4:
                _currentAssessmentStep = 3;
                if ((CurrentPath.PathObjects & PathObjects.Landmarks) == 0)
                {
                    GoToPreviousAssessmentStep();
                    return;
                }
                AssessmentStep = AssessmentStep.LandmarkSelection;
                break;
        }
        Debug.Log($"AssessmentManager :: Go back to step: {_currentAssessmentStep}");
    }

    public void SetBacktrackingMetrics(float actualDistanceToOrigin, float selectedDistanceToOrigin, float absDistanceOffsetToOrigin,
            float bearingErrorToOrigin, float absBearingErrorToOrigin, float backtrackTimeSec, float navigationTimeSec)
    {
        // PathAssessmentData pathAssessment = _currentPathAssessmentData;
        // if (pathAssessment == null)
        // {
        //     Debug.LogWarning($"PathAssessment of path ({CurrentPath.PathID} - {CurrentPath.PathName}) is null;");
        //     return;
        // }
        // pathAssessment.DistanceToStartPosition = distance;        
        _actualDistancetoOrigin = actualDistanceToOrigin;
        _selectedDistancetoOrigin = selectedDistanceToOrigin;
        _absDistanceOffsetToOrigin = absDistanceOffsetToOrigin;
        _bearingErrorToOrigin = bearingErrorToOrigin;
        _absBearingErrorToOrigin = absBearingErrorToOrigin;
        _backtrackTimeSec = backtrackTimeSec;
        _navigationTimeSec = navigationTimeSec;
        Debug.Log($"Distance to Origin {actualDistanceToOrigin}; Selected {selectedDistanceToOrigin}; Offset {absDistanceOffsetToOrigin}");
    }

    public void SetSelectedPathLayout(int selectedPathLayoutID)
    {
        // PathAssessmentData pathAssessment = _currentPathAssessmentData;
        // if (pathAssessment == null)
        // {
        //     Debug.LogWarning($"PathAssessment of path ({CurrentPath.PathID} - {CurrentPath.PathName}) is null;");
        //     return;

        // }
        // pathAssessment.SetPathLayout(selectedPathLayoutID);
        _selectedPathLayout = selectedPathLayoutID;
        _trialSegmentData.ForEach(segment =>
        {
            switch (_selectedPathLayout)
            {
                case 0:
                    segment.SelectedTurnAngleToSegment = segment.ActualTurnAngleToSegment;
                    break;
                case 1:
                    segment.SelectedTurnAngleToSegment = CurrentPath.FakePathAngles1[segment.SegmentNumber];
                    break;
                case 2:
                    segment.SelectedTurnAngleToSegment = CurrentPath.FakePathAngles2[segment.SegmentNumber];
                    break;
                case 3:
                    segment.SelectedTurnAngleToSegment = CurrentPath.FakePathAngles3[segment.SegmentNumber];
                    break;
            }
        });
    }


    public void SetSegmentObjectiveDistance(int segmentID, float distanceValue)
    {
        // SegmentAssessmentData segmentAssessmentData = _currentPathAssessmentData.GetSegment(segmentID);
        // if (segmentAssessmentData == null)
        // {
        //     Debug.LogWarning($"segmentAssessmentData of segment {segmentID} is null");
        // }
        // segmentAssessmentData.SetObjectiveDistance(distanceValue);
        _trialSegmentData[segmentID].SelectedSegmentDistance = distanceValue;
    }

    public void SetSegmentLandmarkData(int segmentID, string landmarkObjectName, float distanceToObjective,
        float differenceToRealPosition, float absDifferenceToRealPosition, float estimatedBearingToLandmark)
    {
        // SegmentAssessmentData segmentAssessmentData = _currentPathAssessmentData.GetSegment(segmentID);
        // segmentAssessmentData.SetLandmarkDistances(distanceToObjective, differenceToRealObject);

        _trialSegmentData[segmentID].SelectedLandmarkObject = landmarkObjectName;
        _trialSegmentData[segmentID].SelectedLandmarkDistance = distanceToObjective;
        _trialSegmentData[segmentID].SignedDistanceOffsetToRealLandmark = differenceToRealPosition;
        _trialSegmentData[segmentID].AbsoluteDistanceOffsetToRealLandmark = absDifferenceToRealPosition;
        _trialSegmentData[segmentID].EstimatedBearingToLandmark = estimatedBearingToLandmark;
    }

    public void AssignSegmentHoverObject(int segmentID, int objectID)
    {
        SegmentAssessmentData segmentAssessmentData = _currentPathAssessmentData.GetSegment(segmentID);
        segmentAssessmentData.SetHoverObject(objectID);
    }

    public void SetSegmentMetrics(int segmentID, float segmenttime, int usedHints, float firstHintTimeSec)
    {
        // _trialSegmentData.Find(segment => segment.SegmentNumber == segmentID).SegmentTimeSec = time;
        _trialSegmentData[segmentID].SegmentTimeSec = segmenttime;
        _trialSegmentData[segmentID].SegmentHints = usedHints;
        _trialSegmentData[segmentID].FirstHintTimeSec = firstHintTimeSec;
    }


    // public void SetSegmentLandmarkObject(int segmentID, int objectID)
    // {
    //     SegmentAssessmentData segmentAssessmentData = _currentPathAssessmentData.GetSegment(segmentID);
    //     segmentAssessmentData.SetLandmarkObject(objectID);
    // }

    public void ResetAssessment()
    {
        CurrentPath = null;
        AssessmentStep = AssessmentStep.Idle;
    }

    public int GetSelectedPathLayout()
    {
        // return Assessment.GetPath(CurrentPath.PathName).SelectedPathLayout;
        return _selectedPathLayout;
    }

}
