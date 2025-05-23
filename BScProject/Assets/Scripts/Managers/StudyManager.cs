using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum StudyState
{
    TrialSetup,
    TrialCompleted,
    StudyCompleted
}

[Serializable]
public class TrialData
{
    public float TrialTimeSec;
    public float NavigationTimeSec;
    public float AssessmentTimeSec;

    [Header("Backtrack Task")]
    public float BacktrackTimeSec;
    public float ActualDistanceToOrigin;
    public float SelectedDistanceToOrigin;
    public float AbsoluteDistanceOffsetToOrigin;
    public float SignedOriginDistanceError;
    public float AbsoluteOriginDistanceError;
    public float SignedBearingErrorToOrigin;
    public float AbsoluteBearingErrorToOrigin;

    [Header("Path Layout Task")]
    public int ActualPathLayout;
    public int SelectedPathLayout;
    public bool IsCorrectPathLayout;

    public TrialData(PathData pathdata)
    {
        ActualPathLayout = pathdata.CorrectPathLayoutID;
    }

    public void CalculateTrialMetrics()
    {
        SignedOriginDistanceError = ActualDistanceToOrigin - SelectedDistanceToOrigin;
        AbsoluteOriginDistanceError = Math.Abs(ActualDistanceToOrigin - SelectedDistanceToOrigin);
        IsCorrectPathLayout = ActualPathLayout == SelectedPathLayout;
    }
}

[Serializable]
public class TrialSegmentData
{
    public int SegmentNumber = -1;
    [Header("General Metrics")]
    public float SegmentTimeSec;
    public float FirstHintTimeSec;
    public float ActualTurnAngleToSegment;
    public float SelectedTurnAngleToSegment;
    public float SignedTurnAngleError;
    public float AbsoluteTurnAngleError;
    public int SegmentHints;

    [Header("Distance Metrics")]
    public float ActualSegmentDistance;
    public float SelectedSegmentDistance;
    public float SignedSegmentDistanceError;
    public float AbsoluteSegmentDistanceError;

    [Header("Landmark Metrics")]
    public float TrueBearingToLandmark;
    public float EstimatedBearingToLandmark;
    public float SignedBearingErrorToLandmark;
    public float AbsoluteBearingErrorToLandmark;
    public float ActualLandmarkDistance;
    public float SelectedLandmarkDistance;
    public float SignedLandmarkDistanceError;
    public float AbsoluteLandmarkDistanceError;
    public float SignedDistanceOffsetToRealLandmark;
    public float AbsoluteDistanceOffsetToRealLandmark;
    public string ActualLandmarkObject;
    public string SelectedLandmarkObject = "None";
    public bool IsCorrectLandmarkObject;

    public TrialSegmentData(int segmentNumber, float actualSegmentDistance,
        float actualAngleToSegment, float actualAngleToLandmark, float actualLandmarkDistance,
        string actualLandmarkObject)
    {
        SegmentNumber = segmentNumber;
        ActualTurnAngleToSegment = actualAngleToSegment;
        ActualSegmentDistance = actualSegmentDistance;
        TrueBearingToLandmark = actualAngleToLandmark;
        ActualLandmarkDistance = actualLandmarkDistance;
        ActualLandmarkObject = actualLandmarkObject;
    }

    public void CalculateSegmentMetrics()
    {
        SignedSegmentDistanceError = ActualSegmentDistance - SelectedSegmentDistance;
        AbsoluteSegmentDistanceError = Math.Abs(SignedSegmentDistanceError);
        SignedLandmarkDistanceError = ActualLandmarkDistance - SelectedLandmarkDistance;
        AbsoluteLandmarkDistanceError = Math.Abs(SignedLandmarkDistanceError);
        SignedTurnAngleError = ActualTurnAngleToSegment - SelectedTurnAngleToSegment;
        AbsoluteTurnAngleError = Math.Abs(SignedTurnAngleError);
        IsCorrectLandmarkObject = ActualLandmarkObject == SelectedLandmarkObject;
        SignedBearingErrorToLandmark = Mathf.DeltaAngle(TrueBearingToLandmark, EstimatedBearingToLandmark);
        AbsoluteBearingErrorToLandmark = Mathf.Abs(SignedBearingErrorToLandmark);
    }
}

public class StudyManager : MonoBehaviour
{
    #region Variables
    public static StudyManager Instance { get; private set; }
    [Header("Study Setup")]
    public StudyData StudyData;
    private UnityEvent<StudyState> _studyStateChanged = new();
    private StudyState _studyState = StudyState.TrialSetup;
    public StudyState StudyState
    {
        get => _studyState;
        set
        {
            _studyState = value;
            _studyStateChanged?.Invoke(value);
        }
    }
    public Timer TrialTimer = new();
    public Timer AssessmentTimer = new();
    private PathData _trialPath;

    [Header("Instructions")]
    [SerializeField]
    private string _trialSetupTextEasy = "Next trial is being prepared, please do not move.\n\n" +
                                        "Let the researcher know when you are ready to continue.";
    [SerializeField]
    private string _trialSetupTextHard = "Next trial is being prepared, please do not move.\n\n" +
                                        "Please let the researcher know when you are ready to continue. \n\n" +
                                        "Remember, highlights disappear when you start moving.";
    [SerializeField]
    private string _studyStartText = "Thank you for participating in this study. \n\n" +
                                    "Please let the researcher know when you are ready to start.";
    [SerializeField]
    private string _studyCompleteText = "Study completed. \n\nThank you for your participation.";

    private string _currentInstruction = "";

    [Header("Study Data")]
    public TrialData TrialData;
    public List<TrialSegmentData> TrialSegments = new();
    public bool IsRunning = false;

    #endregion
    #region Unity Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _studyStateChanged.AddListener(HandleStudyStateChanged);
    }
    
    #endregion
    #region Listener Methods

    private void HandleStudyStateChanged(StudyState new_state)
    {
        switch (new_state)
        {
            case StudyState.TrialSetup:
                if (StudyData.TrialPath.PathDifficulty == PathDifficulty.Easy)
                    _currentInstruction = _trialSetupTextEasy;
                else
                    _currentInstruction = _trialSetupTextHard;
                ShowInstruction();
                break;
            case StudyState.StudyCompleted:
                IsRunning = false;
                _currentInstruction = _studyCompleteText;
                break;
        }
    }

    #endregion
    #region Class Method
    public void ShowInstruction()
    {
        StudySetupManager studySetupManager = FindObjectOfType<StudySetupManager>();
        if (studySetupManager == null)
        {
            Debug.Log($"Could not find study setup manager.");
            return;
        }
        studySetupManager.SetPlayerInstruction(_currentInstruction);
    }

    public void SetupTrial(StudyData studyData)
    {
        this.StudyData = studyData;
        _trialPath = studyData.TrialPath;
        TrialData = new(_trialPath);
        TrialSegments.Clear();
        // SetupTrialSegmentData();
        StartCoroutine(TransitionToScene("ExperimentScene"));
    }

    private void SetupTrialSegmentData()
    {
        TrialSegments.Clear();
        foreach (PathSegmentData pathSegment in _trialPath.SegmentsData)
        {
            string landmarkObjectName = ResourceManager.Instance.GetLandmarkObject(pathSegment.LandmarkObjectID).name.Replace("(Clone)", "").Trim();
            TrialSegments.Add(new(pathSegment.SegmentID + 1, pathSegment.DistanceToPreviousSegment,
                pathSegment.AngleFromPreviousSegment, pathSegment.AngleToLandmark, pathSegment.LandmarkDistanceToSegment,
                landmarkObjectName));
        }
    }

    public void LoadStartScene()
    {
        ResourceManager.Instance.FreeRenderTextures();
        StartCoroutine(TransitionToScene("StartScene"));
    }

    #endregion
    #region Scene Managment
    private IEnumerator TransitionToScene(string sceneName)
    {
        // XROrigin in current scene
        XRFadeTransition currentScene = FindObjectOfType<XRFadeTransition>();
        yield return StartCoroutine(currentScene.Fade(0, 1));

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // XROrigin in target scene
        XRFadeTransition targetScene = FindObjectOfType<XRFadeTransition>();
        yield return StartCoroutine(targetScene.Fade(1, 0));

        PrepareScene();
    }

    private void PrepareScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "ExperimentScene")
        {
            AudioManager.Instance.GetPlayerAudioSource();
            SetPrimaryHand();
            if (ExperimentManager.Instance.SetupExperiment(_trialPath))
            {
                if (_trialPath.PathDifficulty == PathDifficulty.Hard)
                {
                    if (TryGetComponent(out ObjectRenderManager renderManager))
                    {
                        renderManager.InitializeRenderObjects(_trialPath.PathObjects);
                    }
                }
                PathManager.Instance.SetPlayerControllerActions();
                AssessmentManager.Instance.PrepareTrialAssessment(_trialPath);
                ExperimentManager.Instance.StartExperiment();
                if (!IsRunning)
                {
                    IsRunning = true;
                }
            }
            else Debug.LogError($"Experiment could not be prepared.");
        }

        if (currentScene == "StartScene")
        {
            AudioManager.Instance.GetPlayerAudioSource();
            SetPrimaryHand();
            StudyState = StudyState.TrialCompleted;
        }
    }

    private void SetPrimaryHand()
    {
        XRControllerManager controllerManager = FindObjectOfType<XRControllerManager>();
        controllerManager.SetPrimaryHand(StudyData.PrimaryHand);
    }

    public void SetTrialData(float navigationTimeSec, float actualDistancetoOrigin, float selectedDistancetoOrigin, float absDistanceOffsetToOrigin,
        float bearingErrorToOrigin, float absBearingErrorToOrigin, float backtrackTimeSec, int selectedPathLayout, List<TrialSegmentData> trialSegmentData)
    {
        AssessmentTimer.StopTimer();
        TrialData.AssessmentTimeSec = AssessmentTimer.GetTime();
        TrialData.NavigationTimeSec = navigationTimeSec;
        TrialData.TrialTimeSec = TrialTimer.GetTime();
        TrialData.BacktrackTimeSec = backtrackTimeSec;
        TrialData.ActualDistanceToOrigin = actualDistancetoOrigin;
        TrialData.SelectedDistanceToOrigin = selectedDistancetoOrigin;
        TrialData.AbsoluteDistanceOffsetToOrigin = absDistanceOffsetToOrigin;
        TrialData.SignedBearingErrorToOrigin = bearingErrorToOrigin;
        TrialData.AbsoluteBearingErrorToOrigin = absBearingErrorToOrigin;
        TrialData.SelectedPathLayout = selectedPathLayout;
        TrialSegments.AddRange(trialSegmentData);

        TrialData.CalculateTrialMetrics();
        TrialSegments.ForEach(segment => segment.CalculateSegmentMetrics());
    }

    public void CompleteTrial()
    {
        AssessmentTimer.ResetTimer();
        TrialTimer.ResetTimer();
        
        DataManager.Instance.AddTrialLog(TrialData.NavigationTimeSec, TrialData.BacktrackTimeSec, TrialData.AssessmentTimeSec, TrialData.TrialTimeSec);
        DataManager.Instance.ExportBacktrackData(TrialData.BacktrackTimeSec, TrialData.SelectedDistanceToOrigin,
            TrialData.ActualDistanceToOrigin, TrialData.SignedOriginDistanceError, TrialData.AbsoluteOriginDistanceError, TrialData.AbsoluteDistanceOffsetToOrigin,
            TrialData.SignedBearingErrorToOrigin, TrialData.AbsoluteBearingErrorToOrigin);

        foreach (TrialSegmentData segment in TrialSegments)
        {
            DataManager.Instance.ExportAssessmentData(TrialData.SelectedPathLayout, TrialData.ActualPathLayout, TrialData.IsCorrectPathLayout,
                segment.SegmentNumber + 1, segment.SegmentTimeSec, segment.SegmentHints, segment.FirstHintTimeSec, segment.SelectedTurnAngleToSegment,
                segment.ActualTurnAngleToSegment, segment.SelectedSegmentDistance, segment.ActualSegmentDistance,
                segment.SignedSegmentDistanceError, segment.AbsoluteSegmentDistanceError, segment.SelectedLandmarkObject,
                segment.ActualLandmarkObject, segment.IsCorrectLandmarkObject, segment.TrueBearingToLandmark, segment.EstimatedBearingToLandmark,
                segment.SignedBearingErrorToLandmark, segment.AbsoluteBearingErrorToLandmark, segment.SelectedLandmarkDistance,
                segment.ActualLandmarkDistance, segment.SignedLandmarkDistanceError, segment.AbsoluteLandmarkDistanceError,
                segment.SignedDistanceOffsetToRealLandmark, segment.AbsoluteDistanceOffsetToRealLandmark);
        }
        LoadStartScene();
    }

    #endregion

}
