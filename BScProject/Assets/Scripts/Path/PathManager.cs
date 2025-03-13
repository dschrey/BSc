using System;
using UnityEngine;


public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }
    [SerializeField] private GameObject _pathPrefab;
    public Path CurrentPath;
    public PathSegment CurrentSegment;
    public PathSegment LastSegment;
    [SerializeField] private MovementDetection _experimentSpawnMovementDetection;
    private int _unlockedSegments;

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
        _experimentSpawnMovementDetection.ExitedDectectionZone += OnExitedDectectionZoneSpawn;

        // TODO Uncomment
        // CurrentSegment = null;
        // LastSegment = null;
        _unlockedSegments = 0;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnSegmentCompleted()
    {
        if (ExperimentManager.Instance.ExperimentState != ExperimentState.RUNNING)
            return;
        
        _unlockedSegments++;
        if (_unlockedSegments == CurrentPath.Segments.Count)
        {
            ExperimentManager.Instance.PathCompletion?.Invoke();
            return;
        }
        RevealNextPathSegment();
    }

    private void OnExitedDectectionZoneSpawn()
    {
        if (ExperimentManager.Instance.ExperimentState != ExperimentState.RUNNING)
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
            if (LastSegment != null )
                LastSegment.GetComponent<MovementDetection>().ExitedDectectionZone -= OnExitedDectectionZoneSpawn;
            LastSegment = CurrentSegment;
            LastSegment.GetComponent<MovementDetection>().ExitedDectectionZone += OnExitedDectectionZoneSpawn;
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

    [Obsolete]
    public void ClearPath()
    {
        ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();
        objectRenderManager.ClearRenderObjects();
        if (CurrentPath != null)
            Destroy(CurrentPath.gameObject);
        CurrentPath = null;
        CurrentSegment = null;
        LastSegment = null;
        _unlockedSegments = 0;
    }
}
