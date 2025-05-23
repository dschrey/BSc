using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System;

public enum ExperimentState {Idle, Running, Assessment, Finished };

public class ExperimentManager : MonoBehaviour
{
    # region Variables

    public static ExperimentManager Instance { get; private set; }
    public Transform PlayerTransform;
    [HideInInspector] public Transform _XROrigin;
    [HideInInspector] public Transform ExperimentSpawnpoint;
    [HideInInspector] 
    public UnityEvent PathCompletion = new();
    [SerializeField] private GameObject _pathEnvironment;
    [SerializeField] private GameObject _assessmentEnvironment;
    [SerializeField] private ReturnToCenterHandler returnToCenterHandler;
    private UnityEvent<ExperimentState> _experimentStateChanged = new();
    private ExperimentState _experimentState;
    public ExperimentState ExperimentState
    {
        get => _experimentState;
        set
        {
            _experimentState = value;
            _experimentStateChanged?.Invoke(value);
        }
    }

    private Transform _assessmentSpawnPoint;
    private PathData _currentPath;

    #endregion
    #region Unity Methods

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

    private void Start()
    {
        FindOrigin();
        PathCompletion.AddListener(OnPathCompletion);
        _experimentStateChanged.AddListener(OnExperimentStateChanged);
        _experimentState = ExperimentState.Idle;
    }

    void OnDestroy()
    {
        PathCompletion.RemoveListener(OnPathCompletion);
        _experimentStateChanged.RemoveListener(OnExperimentStateChanged);
    }

    #endregion
    # region Listener Methods

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(DataManager.Instance.Settings.MovementArea.y, 3, DataManager.Instance.Settings.MovementArea.x));
    }

    private void OnExperimentStateChanged(ExperimentState newState)
    {
        switch (newState)
        {
            case ExperimentState.Idle:
                break;
            case ExperimentState.Running:
                break;
            case ExperimentState.Assessment:
                FindObjectOfType<UIExperimentInfo>().ToggleAssessmentControl();
                // if (!returnToCenterHandler.InCenter)
                // {
                returnToCenterHandler.PromptReturnToCenter();
                returnToCenterHandler.PlayerInCenter.AddListener(OnPlayerReturnedToCenter);
                // }
                // else
                // {
                    // _pathEnvironment.SetActive(false);
                    // _assessmentEnvironment.SetActive(true);
                //     StartAssessment();
                // }
                break;
            case ExperimentState.Finished:
                // if (!returnToCenterHandler.InCenter)
                // {
                returnToCenterHandler.PromptReturnToCenter();
                returnToCenterHandler.PlayerInCenter.AddListener(OnPlayerReturnedToCenter);
                // }
                // else
                // {
                //     StudyManager.Instance.TrialTimer.StopTimer();
                //     StudyManager.Instance.CompleteTrial();
                // }
                break;
        }
    }

    private void OnPathCompletion()
    {
        BacktrackingCompleted();
        ExperimentState = ExperimentState.Assessment;
    }

    #endregion
    #region Class Methods

    public bool SetupExperiment(PathData path)
    {
        SpawnPoint[] spawnpoints = FindObjectsOfType<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in spawnpoints)
        {
            if (spawnPoint.type == SpawnPointType.Assessment)
                _assessmentSpawnPoint = spawnPoint.transform;
            else if (spawnPoint.type == SpawnPointType.Experiment)
                ExperimentSpawnpoint = spawnPoint.transform;
            else
            {
                _assessmentSpawnPoint = null;
                ExperimentSpawnpoint = null;
            }
        }
        FindOrigin();
        if (_XROrigin == null) return false;

        _currentPath = path;

        return _XROrigin.GetComponentInChildren<LocomotionManager>().SetLocomotionMethod();
    }


    public void StartExperiment()
    {
        if (_XROrigin == null)
        {
            return;
        }

        DataManager.Instance.SetTrackedObject(PlayerTransform);
        PathManager.Instance.StartNewPath(_currentPath, ExperimentSpawnpoint);
        ExperimentState = ExperimentState.Running;
        AudioManager.Instance.ToggleBackgroundLoop();
        // AudioManager.Instance.PlayAudio(SoundType.InstructionUnlockSegmentEasy);
        // if (_currentPath.PathDifficulty == PathDifficulty.Hard)
        // {
        //     AudioManager.Instance.PlayAudio(SoundType.InstructionUnlockSegmentHard);
        // }
    }

    public void StartAssessment()
    {
        AssessmentManager.Instance.StartAssessment();
        // MoveXROrigin(_assessmentSpawnPoint);
    }

    public void StopExperiment()
    {
        StudyManager.Instance.TrialTimer.ResetTimer();
        AssessmentManager.Instance.ResetAssessment();
        StudyManager.Instance.LoadStartScene();
        ExperimentState = ExperimentState.Idle;
    }

    public void MoveXROrigin(Transform destination)
    {
        XRFadeTransition fadeQuad = FindObjectOfType<XRFadeTransition>();
        StartCoroutine(fadeQuad.FadeAndTeleport(destination));
    }

    public void FindOrigin()
    {
        _XROrigin = FindObjectOfType<XROrigin>().transform;
        if (_XROrigin == null)
        {
            Debug.LogError("Could not find XROrigin.");
            return;
        }
    }

    public void TeleportPlayer(Transform target)
    {
        TeleportRequest request = new()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = target.position,
            destinationRotation = target.rotation
        };

        TeleportationProvider m_TeleportationProvider = FindObjectOfType<TeleportationProvider>();
        if (m_TeleportationProvider == null)
        {
            Debug.Log($"No teleport provider found");
            return;
        }

        m_TeleportationProvider.QueueTeleportRequest(request);
    }

    private void BacktrackingCompleted()
    {
        Vector3 originPosition = ExperimentSpawnpoint.position;
        originPosition.y = 0;
        Vector3 selectedBacktrackingPosition = PlayerTransform.position;
        selectedBacktrackingPosition.y = 0;
        Vector3 backtrackStartPosition = PathManager.Instance.LastSegment.transform.position;
        backtrackStartPosition.y = 0;

        float actualDistanceToOrigin = Vector3.Distance(originPosition, backtrackStartPosition);
        float selectedDistanceToOrigin = Vector3.Distance(selectedBacktrackingPosition, backtrackStartPosition);
        float absDistanceOffsetToOrigin = Math.Abs(Vector3.Distance(selectedBacktrackingPosition, originPosition));

        Vector3 trueDirectionToOrigin = (originPosition - backtrackStartPosition).normalized;
        Vector3 selectedDirectionToOrigin = (selectedBacktrackingPosition - backtrackStartPosition).normalized;
        float trueBearingToOrigin = Mathf.Atan2(trueDirectionToOrigin.x, trueDirectionToOrigin.z) * Mathf.Rad2Deg;
        float estimatedBearingToOrigin = Mathf.Atan2(selectedDirectionToOrigin.x, selectedDirectionToOrigin.z) * Mathf.Rad2Deg;

        trueBearingToOrigin = (trueBearingToOrigin + 360f) % 360f;
        estimatedBearingToOrigin = (estimatedBearingToOrigin + 360f) % 360f;

        float signedBearingErrorToOrigin = Mathf.DeltaAngle(estimatedBearingToOrigin, trueBearingToOrigin);
        float absBearingErrorToOrigin = Mathf.Abs(signedBearingErrorToOrigin);

        float backTrackTimeSec = PathManager.Instance.BacktrackTimer.GetTime();
        float navigationTimeSec = PathManager.Instance.NavigationTimer.GetTime();
        AssessmentManager.Instance.SetBacktrackingMetrics(actualDistanceToOrigin, selectedDistanceToOrigin,
            absDistanceOffsetToOrigin, signedBearingErrorToOrigin, absBearingErrorToOrigin, backTrackTimeSec, navigationTimeSec);
    }

    private void OnPlayerReturnedToCenter()
    {
        returnToCenterHandler.PlayerInCenter.RemoveListener(OnPlayerReturnedToCenter);

        switch (_experimentState)
        {
            case ExperimentState.Assessment:
                _pathEnvironment.SetActive(false);
                _assessmentEnvironment.SetActive(true);
                StartAssessment();
                break;
            case ExperimentState.Finished:
                StudyManager.Instance.TrialTimer.StopTimer();
                StudyManager.Instance.CompleteTrial();
                break;
        }
    }

    #endregion
}
