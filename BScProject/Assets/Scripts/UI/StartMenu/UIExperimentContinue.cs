using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIExperimentContinue : MonoBehaviour
{
    [SerializeField] private TMP_Text _experimentID;
    [SerializeField] private TMP_Dropdown _pathDropdown;
    [SerializeField] private Button _buttonContinueExperiment;

    [SerializeField] private Toggle _toggleQuestionaire;
    [SerializeField] private Toggle _toggleFloorChange;
    [SerializeField] private Toggle _toggleReady;

    private bool _questionaireCompleted;
    private bool _hasChangedFloor;
    private bool _isReady;

    private Trail _selectedTrail;
    private PathData _selectedPath;
    private ExperimentData _experiment;
    private AssessmentData _assessment;

    [Header("Debug Controls")]
    [SerializeField] private InputActionReference _debugAction;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Update()
    {
        if (_debugAction != null && _debugAction.action.WasPressedThisFrame())
        {
            OnContinueExperimentClicked();
            return;
        }

        if (_questionaireCompleted && _hasChangedFloor && _isReady)
            _buttonContinueExperiment.interactable = true;
        else
            _buttonContinueExperiment.interactable = false;
    }

    private void OnEnable()
    {
        _questionaireCompleted = false;
        _hasChangedFloor = false;
        _isReady = false;

        _buttonContinueExperiment.onClick.AddListener(OnContinueExperimentClicked);
        _pathDropdown.onValueChanged.AddListener(OnPathSelectionChanged);
        _toggleQuestionaire.onValueChanged.AddListener(OnToggleQuestionaire);
        _toggleFloorChange.onValueChanged.AddListener(OnToggleFloorChange);
        _toggleReady.onValueChanged.AddListener(onToggleReady);
    }

    private void OnDisable()
    {
        _buttonContinueExperiment.onClick.RemoveListener(OnContinueExperimentClicked);
        _pathDropdown.onValueChanged.RemoveListener(OnPathSelectionChanged);
        _toggleQuestionaire.onValueChanged.RemoveListener(OnToggleQuestionaire);
        _toggleFloorChange.onValueChanged.RemoveListener(OnToggleFloorChange);
        _toggleReady.onValueChanged.RemoveListener(onToggleReady);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnContinueExperimentClicked()
    {
        _experiment.paths.Remove(_selectedTrail);
        // StudyManager.Instance.LoadStudyScene(_experiment, _selectedPath, _selectedTrail, _assessment);
        gameObject.SetActive(false);
    }

    private void OnPathSelectionChanged(int index)
    {
        string trailName = _pathDropdown.options[index].text;

        _selectedTrail = _experiment.GetTrailData(trailName);
        if (_selectedTrail == null)
        {
            Debug.LogError("Failed to load selected trail data.");
            return;
        }
        _selectedPath = _experiment.GetPathData(trailName);
        if (_selectedPath == null)
        {
            Debug.LogError("Failed to load selected path data.");
            return;
        }

        Debug.Log($"Experiment {_experiment.id} selected Path: {_selectedPath.PathID} - {_selectedPath.name}");
    }

    private void OnToggleQuestionaire(bool state)
    {
        _questionaireCompleted = state;
    }

    private void OnToggleFloorChange(bool state)
    {
        _hasChangedFloor = state;
    }

    private void onToggleReady(bool state)
    {
        _isReady = state;
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void ContinueExperiment(ExperimentData experiment, AssessmentData assessment)
    {
        _experiment = experiment;
        _assessment = assessment;
        _experimentID.text = experiment.id.ToString();
        PopulatePathOptions(experiment.paths.Keys);
        OnPathSelectionChanged(0);
    }

    private void PopulatePathOptions(Dictionary<Trail, PathData>.KeyCollection trails)
    {
        _pathDropdown.ClearOptions();
        List<string> options = new();
        foreach (Trail trail in trails)
        {
            options.Add(trail.selectionName);
        }
        _pathDropdown.AddOptions(options);
    }

}
