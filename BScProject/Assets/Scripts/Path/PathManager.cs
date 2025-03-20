using System.Collections;
using UnityEngine;


public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }
    [SerializeField] private GameObject _pathPrefab;
    [SerializeField] private GameObject _pathGuidancePrefab;
    public Path CurrentPath;
    public PathSegment CurrentSegment;
    public PathSegment LastSegment;
    [SerializeField] private MovementDetection _experimentSpawnMovementDetection;
    private int _unlockedSegments;
    private Coroutine _guidanceCoroutine;
    public PathGuidance _pathGuidance;
    private bool _pathCompleted;

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
    }

    void OnDestroy()
    {
        if (_experimentSpawnMovementDetection == null)
            _experimentSpawnMovementDetection.PlayerExitedDectectionZone -= OnExitedDectectionZoneSpawn;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentCompleted()
    {
        if (ExperimentManager.Instance.ExperimentState != ExperimentState.Running)
            return;

        RemovePathGuidance();
        _unlockedSegments++;
        if (_unlockedSegments == CurrentPath.Segments.Count)
        {
            _pathCompleted = true;
            // PrepareBacktracking

            ExperimentManager.Instance.PathCompletion?.Invoke();
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

        CurrentSegment.SetObjectiveInvisible();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void StartNewPath(PathData pathData, Transform spawnpoint)
    {
        ExperimentManager.Instance.Timer.StartTimer();
        CurrentPath = Instantiate(_pathPrefab, spawnpoint.position, spawnpoint.rotation).GetComponent<Path>();
        CurrentPath.Initialize(pathData);

        PathLayoutManager.Instance.PreparePathPreviews(CurrentPath.PathData);
        RevealNextPathSegment();
    }

    private void RevealNextPathSegment()
    {
        if (CurrentSegment != null)
        {
            CurrentSegment.SegmentCompleted -= OnSegmentCompleted;
            if (LastSegment != null)
                LastSegment.GetComponent<MovementDetection>().PlayerExitedDectectionZone -= OnExitedDectectionZoneSpawn;
            LastSegment = CurrentSegment;
            LastSegment.GetComponent<MovementDetection>().PlayerExitedDectectionZone += OnExitedDectectionZoneSpawn;
        }
        CurrentSegment = CurrentPath.Segments[_unlockedSegments];
        CurrentSegment.SegmentCompleted += OnSegmentCompleted;
        CurrentSegment.gameObject.SetActive(true);
    }

    public void RestartSegment()
    {
        if (LastSegment != null)
        {
            ExperimentManager.Instance.MoveXROrigin(LastSegment.gameObject.transform);
        }
        else
        {
            ExperimentManager.Instance.MoveXROrigin(ExperimentManager.Instance.ExperimentSpawnpoint);
        }

        CurrentSegment.ShowSegmentObjective();
    }

    public void DisplaySegmentHint()
    {
        if (CurrentSegment == null)
        {
            Debug.LogError($"Cannot display objective hint because there is no active segment.");
            return;
        }
        CurrentSegment.PlaySegmentObjectiveHint();
    }

    public bool VerifyPlayerPosition(Transform player)
    {
        float distance = Vector3.Distance(player.transform.position, CurrentSegment.transform.position);
        return ShowPathGuidance(player, CurrentSegment.transform);
    }

    private bool ShowPathGuidance(Transform player, Transform target)
    {
        if (_guidanceCoroutine != null && ! _pathCompleted)
        {
            return false;
        }

        CurrentSegment.PlayHintAudio();
        _guidanceCoroutine = StartCoroutine(ShowGuidance(player, target));
        return true;
    }

    private void RemovePathGuidance()
    {
        if (_guidanceCoroutine != null)
        {
            StopCoroutine(_guidanceCoroutine);
            Destroy(_pathGuidance.gameObject);
            _guidanceCoroutine = null;
        }
    }

    private IEnumerator ShowGuidance(Transform player, Transform target)
    {
        _pathGuidance = Instantiate(_pathGuidancePrefab).GetComponent<PathGuidance>();
        _pathGuidance.Initialize(player, target);
        yield return new WaitForSeconds(DataManager.Instance.Settings.PathGuidanceCooldown);
        RemovePathGuidance();
    }
}
