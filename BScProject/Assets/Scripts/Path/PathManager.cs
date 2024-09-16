using System.Collections.Generic;
using UnityEngine;


public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }
    [SerializeField] private GameObject _pathPrefab;
    public Path CurrentPath;
    public PathSegment CurrentSegment;
    public PathSegment LastSegment;
    public List<PathSegment> UnlockedSegments;

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
        Debug.Log($"PathManager :: OnSegmentCompleted() : Revealing new segment.");
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


    public void StartNewPath(PathData pathData)
    {
        if (CurrentPath != null)
        {
            Destroy(CurrentPath.gameObject);
        }
        
        CurrentPath = Instantiate(_pathPrefab, ExperimentManager.Instance.ExperimentSpawn.position, ExperimentManager.Instance.ExperimentSpawn.rotation).GetComponent<Path>();
        
        CurrentPath.SetupPathSegments(pathData);

        _unlockedSegments = 0;
        TeleportPlayerToPathStart();
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
        if (CurrentSegment != null)
        {
            ExperimentManager.Instance._XROrigin.SetPositionAndRotation(LastSegment.gameObject.transform.position, LastSegment.gameObject.transform.rotation);
        }
        else
        {
            ExperimentManager.Instance._XROrigin.SetPositionAndRotation(ExperimentManager.Instance.ExperimentSpawn.position, ExperimentManager.Instance.ExperimentSpawn.rotation);
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

    private void TeleportPlayerToPathStart()
    {
        ExperimentManager.Instance._XROrigin.SetPositionAndRotation(ExperimentManager.Instance.ExperimentSpawn.position, ExperimentManager.Instance.ExperimentSpawn.rotation);
    }
}
