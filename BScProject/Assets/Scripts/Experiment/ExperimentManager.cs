using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public enum ExperimentState {IDLE, LOADPATH, RUNNING, EVALUATION, FINISHED, CANCELLED };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }
    public ExperimentSettings ExperimentSettings;
    public List<PathData> Paths = new();
    [SerializeField] private UIExperimentPanelManager _UIExperimentPanelManager;
    [SerializeField] private Transform _evaluationRoomSpawnPoint;
    public Transform _XROrigin;
    public Transform ExperimentSpawn;
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
    
    private int _pathCount;
    public bool PathAvailable => _pathCount != Paths.Count;



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
            case ExperimentState.EVALUATION:
                StartAssessment();
				break;
            case ExperimentState.FINISHED:
                AssessmentManager.Instance.Assessment.Completed = true;
                DataManager.Instance.SaveAssessmentData(AssessmentManager.Instance.Assessment);
                AssessmentManager.Instance.Assessment = null;
                DataManager.Instance.AssessmentFile = null;
                StopExperiment();
                break;
		}
    }

    private void OnPathCompletion()
    {   
        ExperimentState = ExperimentState.EVALUATION;
    }

    
    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------
    
    public void StartExperiment()
    {
        Debug.Log($"ExperimentManager :: StartExperiment() : Starting Experiment..");
        _UIExperimentPanelManager.CloseSetupPanel();
        _pathCount = 0;
        PathManager.Instance.StartNewPath(Paths[_pathCount]);
        ExperimentState = ExperimentState.RUNNING;
    }

    private void LoadNextPath()
    {
        _pathCount++;
        if (! PathAvailable)
        {
            ExperimentState = ExperimentState.FINISHED;
            return;
        }
        Debug.Log($"ExperimentManager :: StartNextPath() : Getting next Path.");
        PathManager.Instance.StartNewPath(Paths[_pathCount]);
        
        ExperimentState = ExperimentState.RUNNING;
    }

    private void StartAssessment()
    {
        Debug.Log($"Starting assessment for path ID: {PathManager.Instance.CurrentPath.PathData.PathID}");
        AssessmentManager.Instance.StartPathAssessment(PathManager.Instance.CurrentPath.PathData);
        _XROrigin.SetLocalPositionAndRotation(_evaluationRoomSpawnPoint.position, _evaluationRoomSpawnPoint.rotation);
    }

    public void StopExperiment()
    {
        _pathCount = 0;
        _XROrigin.SetPositionAndRotation(ExperimentSpawn.position, ExperimentSpawn.rotation);
        _UIExperimentPanelManager.ToggleRunningPanel(false);
        _UIExperimentPanelManager.OpenSetupPanel();
        ExperimentState = ExperimentState.IDLE;
    }

    public void PathAssessmentCompleted()
    {
        ExperimentState = ExperimentState.IDLE;
        LoadNextPath();
    }
}
