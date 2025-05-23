using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PathManager : MonoBehaviour
{

    #region Variables

    public static PathManager Instance { get; private set; }
    [SerializeField] private GameObject _pathPrefab;
    [SerializeField] private GameObject _pathGuidancePrefab;
    public Path CurrentPath;
    public PathSegment CurrentSegment;
    public PathSegment LastSegment;
    private readonly Timer _segmentTimer = new();
    private readonly Timer _hintUsageTimer = new();
    private readonly Timer _hintAvailableTimer = new();
    [SerializeField] float _timeHintAvailable = 4f;
    public readonly Timer BacktrackTimer = new();
    public readonly Timer NavigationTimer = new();
    [SerializeField] private MovementDetection _experimentSpawnMovementDetection;
    private int _unlockedSegments;
    private Coroutine _guidanceCoroutine;
    private PathGuidance _pathGuidance;
    private bool _pathCompleted;
    private bool _backtrackingCompleted;
    [Header("Player Controlls"), SerializeField] private InputActionReference _playerConfirmAction;
    [SerializeField] private InputActionReference _debugConfirmAction;
    public int HintCounter { get; private set; }

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
        if (_experimentSpawnMovementDetection == null)
        {
            Debug.LogError($"Could not find experiment Spawnpoint movement detection.");
            return;
        }

        _experimentSpawnMovementDetection.PlayerExitedDectectionZone += OnExitedDectectionZoneSpawn;

        CurrentSegment = null;
        LastSegment = null;
        _unlockedSegments = 0;
        _pathCompleted = false;
        _backtrackingCompleted = false;
    }

    private void Update()
    {
        if ((_playerConfirmAction != null && _playerConfirmAction.action.WasPressedThisFrame()) ||
            _debugConfirmAction != null && _debugConfirmAction.action.WasPressedThisFrame())
        {
            if (!_pathCompleted && !_backtrackingCompleted)
            {
                if (CurrentPath.PathData.PathDifficulty == PathDifficulty.Easy) return;
                RecordHint();
            }
            else if (_pathCompleted && !_backtrackingCompleted)
            {
                DataManager.Instance.ToggleObjectTracking(false);
                _backtrackingCompleted = true;
                BacktrackTimer.StopTimer();
                ExperimentManager.Instance.PathCompletion?.Invoke();
                DataManager.Instance.ExportMovementData(ExportEvent.BACKTRACK_FINISHED);
            }
        }
    }

    private void RecordHint()
    {
        HintCounter++;
        _hintUsageTimer.StopTimer();
        if (HintCounter >= DataManager.Instance.Settings.MaxNumberOfHints)
        {
            CurrentSegment.SetParticleVisuals(2, true);
            HintCounter = DataManager.Instance.Settings.MaxNumberOfHints;
        }
        else
        {
            DataManager.Instance.ExportMovementData(ExportEvent.HINT);
            CurrentSegment.ShowSegmentHint();
        }
    }

    void OnDestroy()
    {
        if (_experimentSpawnMovementDetection == null)
            _experimentSpawnMovementDetection.PlayerExitedDectectionZone -= OnExitedDectectionZoneSpawn;
    }

    #endregion
    #region Listener Methods

    private void OnSegmentCompleted()
    {
        if (ExperimentManager.Instance.ExperimentState != ExperimentState.Running)
            return;

        AssessmentManager.Instance.SetSegmentMetrics(CurrentSegment.PathSegmentData.SegmentID,
            _segmentTimer.GetTime(), HintCounter, _hintUsageTimer.GetTime());
        DataManager.Instance.ExportMovementData(ExportEvent.SEGMENT_COMPLETED);
        _unlockedSegments++;
        if (_unlockedSegments == CurrentPath.Segments.Count)
        {
            _pathCompleted = true;
            LastSegment = CurrentSegment;
            CurrentSegment = null;
            AudioManager.Instance.ToggleBackgroundLoop();
            DataManager.Instance.SetTrackingReferenceObject(ExperimentManager.Instance.ExperimentSpawnpoint);
            DataManager.Instance.SetExportEvent(ExportEvent.BACKTRACK);
            ExperimentManager.Instance.ExperimentSpawnpoint.GetComponent<SpawnPoint>().ToggleHighlight(false);
            CurrentPath.Segments.ForEach(segment => segment.SetParticleVisuals(-1, false));

            if (StudyManager.Instance.StudyData.PrimaryHand == PrimaryHand.Right)
            {
                AudioManager.Instance.PlayAudio(SoundType.InstructionBackTrackRight);
            }
            else
            {
                AudioManager.Instance.PlayAudio(SoundType.InstructionBackTrackLeft);
            }

            NavigationTimer.StopTimer();
            BacktrackTimer.StartTimer();
            return;
        }
        RevealNextPathSegment();
    }

    private void OnExitedDectectionZoneSpawn()
    {
        if (ExperimentManager.Instance.ExperimentState != ExperimentState.Running)
            return;

        if (CurrentSegment == null)
        {
            Debug.LogError($"Exited detection zone but there is no active segment.");
            return;
        }

        if (CurrentPath.PathData.PathDifficulty == PathDifficulty.Hard)
            CurrentSegment.SetSegmentInvisible();
    }

    #endregion
    #region Class Methods

    public void SetPlayerControllerActions()
    {
        XRControllerManager controllerManager = FindObjectOfType<XRControllerManager>();
        if (controllerManager == null)
        {
            Debug.LogError($"Could not find XR controller manager.");
            return;
        }
        _playerConfirmAction = controllerManager.ActiveActivateAction;
        _debugConfirmAction = controllerManager.DebugActivateAction;
    }

    public void StartNewPath(PathData pathData, Transform spawnpoint)
    {

        CurrentPath = Instantiate(_pathPrefab, spawnpoint.position, spawnpoint.rotation).GetComponent<Path>();
        CurrentPath.Initialize(pathData);

        DataManager.Instance.SetExportEvent(ExportEvent.NAVIGATION);
        StudyManager.Instance.TrialTimer.StartTimer();
        NavigationTimer.StartTimer();

        PathLayoutManager.Instance.PreparePathPreviews(CurrentPath.PathData);
        RevealNextPathSegment();
    }

    private void RevealNextPathSegment()
    {
        if (CurrentSegment != null)
        {
            CurrentSegment.SegmentCompleted -= OnSegmentCompleted;
            if (LastSegment != null)
            {
                LastSegment.SetParticleVisuals(-1, false);
                LastSegment.GetComponent<MovementDetection>().PlayerExitedDectectionZone -= OnExitedDectectionZoneSpawn;
            }
            {
                // If there is no segment before the most recent, then it must be the start position
                ExperimentManager.Instance.ExperimentSpawnpoint.GetComponent<SpawnPoint>().ToggleHighlight(false);
            }
            LastSegment = CurrentSegment;
            LastSegment.GetComponent<MovementDetection>().PlayerExitedDectectionZone += OnExitedDectectionZoneSpawn;
        }

        HintCounter = 0;
        _hintAvailableTimer.RestartTimer();
        _hintUsageTimer.RestartTimer();
        _segmentTimer.RestartTimer();
        CurrentSegment = CurrentPath.Segments[_unlockedSegments];

        DataManager.Instance.SetTrackingReferenceObject(CurrentSegment.gameObject.transform);
        DataManager.Instance.ToggleObjectTracking(true);

        CurrentSegment.SegmentCompleted += OnSegmentCompleted;
        if (CurrentPath.PathData.PathDifficulty == PathDifficulty.Easy)
        {
            CurrentSegment.SetParticleVisuals(1, false);
            CurrentSegment.SetParticleVisuals(2, true);
        }
        else
        {
            CurrentSegment.SetParticleVisuals(1, true);
            CurrentSegment.SetObjectsVisuals(1, true);
        }
    }

    public void DisplaySegmentHint()
    {
        if (CurrentSegment == null)
        {
            Debug.LogError($"Cannot display objective hint because there is no active segment.");
            return;
        }
        CurrentSegment.ShowSegmentHint();
    }

    // public bool VerifyPlayerPosition(Transform player)
    // {
    //     return ShowPathGuidance(player, CurrentSegment.transform);
    // }

    // private bool ShowPathGuidance(Transform player, Transform target)
    // {
    //     if (_guidanceCoroutine != null && !_pathCompleted)
    //     {
    //         return false;
    //     }

    //     AudioManager.Instance.PlayAudio(SoundType.SoundSegmentHint);
    //     _guidanceCoroutine = StartCoroutine(ShowGuidance(player, target));
    //     return true;
    // }

    private void RemovePathGuidance()
    {
        if (_guidanceCoroutine != null)
        {
            StopCoroutine(_guidanceCoroutine);
            Destroy(_pathGuidance.gameObject);
            _guidanceCoroutine = null;
        }
    }

    [Obsolete]
    private IEnumerator ShowGuidance(Transform player, Transform target)
    {
        _pathGuidance = Instantiate(_pathGuidancePrefab).GetComponent<PathGuidance>();
        _pathGuidance.Initialize(player, target);
        yield return new WaitForSeconds(DataManager.Instance.Settings.PathGuidanceCooldown);
        RemovePathGuidance();
    }

    public int GetCurrentSegmentID()
    {
        if (CurrentSegment != null)
            return CurrentSegment.PathSegmentData.SegmentID + 1;
        else
            return 0;
    }
    public Transform GetLastSegment()
    {
        if (LastSegment != null)
            return LastSegment.transform;
        else
            return ExperimentManager.Instance.ExperimentSpawnpoint;
    }
    
    #endregion
}
