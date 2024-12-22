using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public enum ExperimentState {IDLE, LOADPATH, RUNNING, ASSESSMENT, FINISHED, CANCELLED };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }
    public ExperimentSettings ExperimentSettings;
    public List<PathData> Paths = new();
    public Transform AssessmentRoomSpawnPoint;
    [SerializeField] private UIExperimentPanelManager _UIExperimentPanelManager;
    public Transform _XROrigin;
    public Transform ExperimentSpawn;
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
    
    public int CompletedPaths;
    public bool PathAvailable => CompletedPaths != Paths.Count;




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
        _XROrigin = FindObjectOfType<XROrigin>().transform;
        if (_XROrigin == null)
        {
            Debug.LogError("ExperimentManager :: Start() : Could not find XROrigin.");
            return;
        }

        PathCompletion.AddListener(OnPathCompletion);
        _experimentStateChanged.AddListener(OnExperimentStateChanged);
        _experimentState = ExperimentState.IDLE;
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
                StartAssessment();
				break;
            case ExperimentState.FINISHED:
                AssessmentManager.Instance.FinishAssessment();
                PathManager.Instance.ClearPath();
                StopExperiment();
                break;
		}
    }

    private void OnPathCompletion()
    {
        Timer.Stop();
        ExperimentState = ExperimentState.ASSESSMENT;
    }

    
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
    
    public void StartExperiment()
    {
        Debug.Log($"ExperimentManager :: StartExperiment() : Starting Experiment..");
        _UIExperimentPanelManager.CloseSetupPanel();
        CompletedPaths = 0;
        PathManager.Instance.StartNewPath(Paths[CompletedPaths]);
        ExperimentState = ExperimentState.RUNNING;
    }

    private void LoadNextPath()
    {
        CompletedPaths++;
        if (! PathAvailable)
        {
            ExperimentState = ExperimentState.FINISHED;
            return;
        }
        Debug.Log($"ExperimentManager :: StartNextPath() : Getting next Path.");
        Timer.Reset();
        PathManager.Instance.StartNewPath(Paths[CompletedPaths]);
        
        ExperimentState = ExperimentState.RUNNING;
    }

    private void StartAssessment()
    {
        Debug.Log($"Starting assessment for path ID: {PathManager.Instance.CurrentPath.PathData.PathID}");
        AssessmentManager.Instance.StartPathAssessment(PathManager.Instance.CurrentPath.PathData);
        MoveXROrigin(AssessmentRoomSpawnPoint);
    }

    public void StopExperiment()
    {
        CompletedPaths = 0;
        MoveXROrigin(ExperimentSpawn);
        _UIExperimentPanelManager.ToggleRunningPanel(false);
        _UIExperimentPanelManager.ResetPanelPosition();
        _UIExperimentPanelManager.ShowSetupPanel();
        ExperimentState = ExperimentState.IDLE;
    }

    public void PathAssessmentCompleted()
    {
        ExperimentState = ExperimentState.IDLE;
        LoadNextPath();
    }

    public void MoveXROrigin(Transform target)
    {
        TeleportFade fadeQuad = FindObjectOfType<TeleportFade>();
        StartCoroutine(fadeQuad.FadeAndTeleport(0, 1, target));
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

        Debug.Log($"Teleport to: {request.destinationPosition}");

        TeleportationProvider m_TeleportationProvider = FindObjectOfType<TeleportationProvider>();
        if (m_TeleportationProvider == null)
        {
            Debug.Log($"No teleport provider found");
            return;
        }
        
        m_TeleportationProvider.QueueTeleportRequest(request);
    }

}
