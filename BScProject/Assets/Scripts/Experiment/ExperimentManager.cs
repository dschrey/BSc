using Omnifinity.Omnideck;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using System.Collections;
using UnityEngine.InputSystem;

public enum ExperimentState {IDLE, RUNNING, ASSESSMENT, FINISHED, CANCELLED };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }
    [SerializeField] private UIExperimentPanelManager _UIExperimentPanelManager;
    public Transform _XROrigin;
    public Transform ExperimentSpawnpoint;
    public Timer Timer;
    public UnityEvent PathCompletion = new();
    private UnityEvent<ExperimentState> _experimentStateChanged = new();
    private ExperimentState _experimentState = ExperimentState.IDLE;
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
    public PathData _currentPath;
    public ExperimentData CurrentExperiment;
    [SerializeField] private OmnideckInterface _omnideckInterface;
    [SerializeField] public InputActionReference StartAction;




    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
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
        _experimentState = ExperimentState.IDLE;
    }

    private void Update()
    {
        if (StartAction != null && StartAction.action.WasPressedThisFrame())
        {
            StartAssessment();
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExperimentStateChanged(ExperimentState newState)
    {
        switch (newState)
        {
            case ExperimentState.IDLE:
                break;
            case ExperimentState.RUNNING:
                break;
            case ExperimentState.ASSESSMENT:
                FindObjectOfType<UIExperimentInfo>().ToggleAssessmentControl();

                // TODO Remove after testing
                //StartCoroutine(AssessmentStart());
                break;
            case ExperimentState.FINISHED:
                _experimentState = ExperimentState.IDLE;
                SceneManager.Instance.LoadStartScene(ExperimentState);
                break;
        }
    }

    private void OnPathCompletion()
    {
        Timer.Stop();
        Debug.Log($"Path Completed");
        ExperimentState = ExperimentState.ASSESSMENT;
    }


    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public bool SetupExperiment(ExperimentData experiment, PathData path, FloorType floor)
    {
        SpawnPoint[] spawnpoints = FindObjectsOfType<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in spawnpoints)
        {
            if (spawnPoint.type == SpawnPointType.ASSESSMENT)
                _assessmentSpawnPoint = spawnPoint.transform;
            else if (spawnPoint.type == SpawnPointType.EXPERIMENT)
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

        return _XROrigin.GetComponentInChildren<MovementManager>().HandleFloorBasedMovement(floor);
    }

    public void StartExperiment()
    {
        if (_XROrigin == null)
        {
            return;
        }

        PathManager.Instance.StartNewPath(_currentPath, ExperimentSpawnpoint);
        ExperimentState = ExperimentState.RUNNING;
    }

    public void StartAssessment()
    {
        Debug.Log($"Starting assessment for path ID: {_currentPath.PathID}");
        AssessmentManager.Instance.StartAssessment();
        MoveXROrigin(_assessmentSpawnPoint);
    }

    public void StopExperiment()
    {
        Timer.Reset();
        AssessmentManager.Instance.ResetAssessment();
        SceneManager.Instance.LoadStartScene(ExperimentState.CANCELLED);
        ExperimentState = ExperimentState.IDLE;
    }

    public void PathAssessmentCompleted()
    {
        ExperimentState = ExperimentState.FINISHED;
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


    // TODO REMOVE

    private IEnumerator AssessmentStart()
    {
        yield return new WaitForSeconds(10);
        StartAssessment();
    }

}
