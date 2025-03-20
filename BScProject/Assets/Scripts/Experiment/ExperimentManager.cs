using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.InputSystem;

public enum ExperimentState {Idle, Running, Assessment, Finished, Cancelled };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }
    public Transform PlayerTransform;
    [HideInInspector] public Transform _XROrigin;
    [HideInInspector] public Transform ExperimentSpawnpoint;
    [HideInInspector] public UnityEvent PathCompletion = new();
    public Timer Timer;
    public ExperimentData CurrentExperiment;
    private UnityEvent<ExperimentState> _experimentStateChanged = new();
    private ExperimentState _experimentState = ExperimentState.Idle;
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

    [SerializeField] private InputActionReference _confirmAction;
    private int _hintCounter;

    [Header("Debug"), SerializeField]
    private InputActionReference _debugAction;

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
        FindOrigin();
        PathCompletion.AddListener(OnPathCompletion);
        _experimentStateChanged.AddListener(OnExperimentStateChanged);
        _experimentState = ExperimentState.Idle;
    }

    private void Update()
    {
        if (_debugAction != null && _debugAction.action.WasPressedThisFrame())
        {
            OnPathCompletion();
            StartAssessment();
        }

        if (_confirmAction != null && _confirmAction.action.WasPressedThisFrame())
        {
            CheckPlayerPosition();
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

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
                break;
            case ExperimentState.Finished:
                _experimentState = ExperimentState.Idle;
                SceneManager.Instance.LoadStartScene(ExperimentState);
                break;
        }
    }

    private void OnPathCompletion()
    {
        Timer.Stop();
        ExperimentState = ExperimentState.Assessment;
    }


    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public bool SetupExperiment(ExperimentData experiment, PathData path, FloorType floor)
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

        CurrentExperiment = experiment;
        _currentPath = path;
        _hintCounter = 0;

        return _XROrigin.GetComponentInChildren<LocomotionManager>().HandleFloorBasedMovement(floor);
    }

    public void StartExperiment()
    {
        if (_XROrigin == null)
        {
            return;
        }

        PathManager.Instance.StartNewPath(_currentPath, ExperimentSpawnpoint);
        ExperimentState = ExperimentState.Running;
    }

    public void StartAssessment()
    {
        Debug.Log($"Starting assessment for path ID: {_currentPath.PathID}");
        AssessmentManager.Instance.StartAssessment(_hintCounter);
        MoveXROrigin(_assessmentSpawnPoint);
    }

    public void StopExperiment()
    {
        Timer.Reset();
        AssessmentManager.Instance.ResetAssessment();
        SceneManager.Instance.LoadStartScene(ExperimentState.Cancelled);
        ExperimentState = ExperimentState.Idle;
    }

    public void PathAssessmentCompleted()
    {
        ExperimentState = ExperimentState.Finished;
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
    
    private void CheckPlayerPosition()
    {
        if (PathManager.Instance.VerifyPlayerPosition(PlayerTransform))
        {
            _hintCounter++;
            Debug.Log($"Displayig guidance - Counter: {_hintCounter}");
        }
    }

}
