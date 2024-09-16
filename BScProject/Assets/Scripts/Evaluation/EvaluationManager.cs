using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

public enum EvaluationStep {IDLE, PATHSELECTION, ITEMDISTANCE, OBSTACLELOCATION, OBSTACLEDISTANCE, COMPLETED }

public class EvaluationManager : MonoBehaviour
{
    public static EvaluationManager Instance { get; private set; }
    public ExperimentSettings EvaluationSettings;
    private UnityEvent<EvaluationStep> _evaluationStepChanged = new();
    private EvaluationStep _evaluationStep;
    public EvaluationStep EvaluationStep
    {
        get => _evaluationStep;
        set 
        {
            _evaluationStep = value;
            _evaluationStepChanged?.Invoke(value);
        }
    }
    [SerializeField] private GameObject _pathSelectionPanel;
    [SerializeField] private GameObject _itemDistancePanel;
    [SerializeField] private GameObject _obstacleLocationPanel;
    [SerializeField] private GameObject _obstacleDistancePanel;
    private GameObject _activePanel;
    private int _currentEvaluationStep;

    [Header("Evaluation Results (in current Iteration)")]

    // TODO EvaluiationResults class
    public PathData SelectedPath;

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

    void Start()
    {
        _evaluationStepChanged.AddListener(OnEvaluationStepChanged);
        _evaluationStep = EvaluationStep.IDLE;

    }

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEvaluationStepChanged(EvaluationStep newStep)
    {
        if (_activePanel != null)
            _activePanel.SetActive(false);
        switch (newStep)
        {
            case EvaluationStep.IDLE:
                _activePanel = null;
                break;
            case EvaluationStep.PATHSELECTION:
                _activePanel = _pathSelectionPanel;
				break;
            case EvaluationStep.ITEMDISTANCE:
                _activePanel = _itemDistancePanel;
				break;
            case EvaluationStep.OBSTACLELOCATION:
                _activePanel = _obstacleLocationPanel;
				break;
            case EvaluationStep.OBSTACLEDISTANCE:
                _activePanel = _obstacleDistancePanel;
				break;
            case EvaluationStep.COMPLETED:
                _activePanel = null;
				break;
		}
        if (_activePanel != null)
            _activePanel.SetActive(true);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    public void ProceedToNextEvaluationStep()
    {
        if (_currentEvaluationStep == EvaluationSettings.MaxEvaluationSteps)
        {
            EvaluationStep = EvaluationStep.COMPLETED;
        }

        switch (EvaluationSettings.EvaluationType)
        {
            case ExperimentType.STANDARD:
                switch (_currentEvaluationStep)
                {
                    case 0:
                        EvaluationStep = EvaluationStep.PATHSELECTION;
                        break;
                    case 1:
                        EvaluationStep = EvaluationStep.ITEMDISTANCE;
                        break;
                }
                break;
            case ExperimentType.EXTENDED:
                switch (_currentEvaluationStep)
                {
                    case 0:
                        EvaluationStep = EvaluationStep.PATHSELECTION;
                        break;
                    case 1:
                        EvaluationStep = EvaluationStep.OBSTACLEDISTANCE;
                        break;
                    case 2:
                        EvaluationStep = EvaluationStep.OBSTACLELOCATION;
                        break;
                }
                break;
        }
        _currentEvaluationStep++;
    }

}
