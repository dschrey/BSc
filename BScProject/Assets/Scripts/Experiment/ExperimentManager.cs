using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public enum ExperimentState {IDLE, LOADPATH, RUNNING, EVALUATION, FINISHED};

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
            // case ExperimentState.LOADPATH:
            //     LoadNextPath();
				break;
            case ExperimentState.RUNNING:
                break;
            case ExperimentState.EVALUATION:
                StartAssessment();
				break;
            case ExperimentState.FINISHED:
                _UIExperimentPanelManager.ShowFinishPanel();
				break;
		}
    }

    private void OnPathCompletion()
    {    
        ExperimentState = ExperimentState.EVALUATION;
    }

    private void OnPathEvaluationCompleted()
    {
        // ExperimentState = ExperimentState.LOADPATH;
        LoadNextPath();
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
        AssessmentManager.Instance.StartPathAssessment(PathManager.Instance.CurrentPath.PathData);
        _XROrigin.SetLocalPositionAndRotation(_evaluationRoomSpawnPoint.position, _evaluationRoomSpawnPoint.rotation);
    }

    internal void StopExperiment()
    {
        // TODO 
        throw new NotImplementedException();
    }
}
