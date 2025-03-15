using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UINewExperiment : MonoBehaviour
{
    [Header("UI Elements"), SerializeField] private TMP_InputField _inputExperimentID;
    [SerializeField] private Button _buttonEditExperimentID;
    [SerializeField] private TMP_Dropdown _pathDropdown;
    [SerializeField] private Button _buttonStartExperiment;
    [SerializeField] private Toggle _toggleResetExperimentCount;
    [SerializeField] private Toggle _toggleResetAssessmentData;
    [SerializeField] private Button _buttonOmnideckStatus;
    [SerializeField] private TMP_Text _textOmnideckStatusButton;
    [SerializeField] private TMP_Text _textOmnideckStatus;
    [SerializeField] private Transform _interfaceParent;
    [SerializeField] private GameObject _omnideckInterfacePrefab;
    private Trail _selectedTrail;
    private PathData _selectedPath;
    private int _experimentID;
    private ExperimentData _experiment;
    private bool _overwriteCompletedExperiment = false;
    private bool _overwriteAssessmentData = false;
    public AssessmentData _currentAssessment = null;

    [Header("Debug"), SerializeField]
    private InputActionReference _debugAction;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _inputExperimentID.onEndEdit.AddListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.AddListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.AddListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.AddListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.AddListener(OnResetExperimentCount);
        _toggleResetAssessmentData.onValueChanged.AddListener(OnResetAssessmentData);
    }

    private void OnDisable()
    {
        _inputExperimentID.onEndEdit.RemoveListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.RemoveListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.RemoveListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.RemoveListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.RemoveListener(OnResetExperimentCount);
        _toggleResetAssessmentData.onValueChanged.RemoveListener(OnResetAssessmentData);
    }

    void Start()
    {
        _experimentID = DataManager.Instance.Settings.CompletedExperiments;
        _inputExperimentID.text = _experimentID.ToString();
        _currentAssessment = DataManager.Instance.TryLoadAssessmentData(_experimentID);
        if (_currentAssessment == null) _toggleResetAssessmentData.isOn = false;
        _toggleResetAssessmentData.interactable = _currentAssessment != null;
        PopulatePathOptions();
    }

    private void Update()
    {
        if (_debugAction != null && _debugAction.action.WasPressedThisFrame())
        {
            OnStartExperimentClicked();
        }
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExperimentIDChanged(string input)
    {
        int.TryParse(input, out int value);
        _experimentID = value;
        _currentAssessment = DataManager.Instance.TryLoadAssessmentData(_experimentID);
        if (_currentAssessment == null) _toggleResetAssessmentData.isOn = false;
        _toggleResetAssessmentData.interactable = _currentAssessment != null;
        PopulatePathOptions();
    }

    private void OnEditExperimentIDClicked()
    {
        bool enabled = _inputExperimentID.interactable;
        _toggleResetExperimentCount.gameObject.SetActive(!enabled);
        _toggleResetAssessmentData.gameObject.SetActive(!enabled);
        _inputExperimentID.interactable = !enabled;
    }

    private void OnStartExperimentClicked()
    {
        if (_overwriteCompletedExperiment)
        {
            DataManager.Instance.Settings.CompletedExperiments = _experimentID;
            DataManager.Instance.SaveSettings();
        }

        _experiment.paths.Remove(_selectedTrail);

        if (_overwriteAssessmentData || _currentAssessment == null)
        {
            SceneManager.Instance.LoadExperimentScene(_experiment, _selectedPath, _selectedTrail, new(_experimentID, DateTime.Now));
        }
        else
        {
            SceneManager.Instance.LoadExperimentScene(_experiment, _selectedPath, _selectedTrail, _currentAssessment);
        }

        gameObject.SetActive(false);
    }

    private void OnPathSelectionChanged(int index)
    {
        if (_pathDropdown.options.Count == 0) return;
        string trailName = _pathDropdown.options[index].text;

        _selectedTrail = _experiment.GetTrailData(trailName);
        if (_selectedTrail == null)
        {
            Debug.LogWarning("Failed to load selected trail data.");
            return;
        }
        _selectedPath = _experiment.GetPathData(trailName);
        if (_selectedPath == null)
        {
            Debug.LogWarning("Failed to load selected path data.");
            return;
        }

        _buttonStartExperiment.interactable = true;
        Debug.Log($"Experiment {_experiment.id}: {_selectedPath.name} - {_selectedTrail.floor}");
    }

    private void OnResetExperimentCount(bool state)
    {
        _overwriteCompletedExperiment = state;
    }

    private void OnResetAssessmentData(bool state)
    {
        _overwriteAssessmentData = state;
        _buttonStartExperiment.interactable = state;
        PopulatePathOptions();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void PopulatePathOptions()
    {
        _pathDropdown.ClearOptions();
        List<string> options = new();
        ParticipantData data = ResourceManager.Instance.GetExperimentData(_experimentID);
        if (data == null)
        {
            Debug.LogError($"Failed to load experiment data for id: {_experimentID}.");
            options.Add("No path found");
            _pathDropdown.AddOptions(options);
            return;
        }

        _experiment = new(_experimentID);

        if (_currentAssessment != null && !_overwriteAssessmentData)
        {
            if (_currentAssessment.Completed)
            {
                options.Add("Completed");
                _buttonStartExperiment.interactable = false;
                _pathDropdown.AddOptions(options);
                return;
            }
        }

        List<string> completedPaths = new();
        _currentAssessment?.Paths.ForEach(path =>
        {
            string floor = path.FloorType == 0 ? "Reg" : "Omni";
            string selectionName = $"{_currentAssessment.AssessmentID}_{path.Name}_{floor}";
            completedPaths.Add(selectionName);
        });

        foreach (Trail trail in data.paths)
        {
            string completedPath = completedPaths.Find(name => name == trail.selectionName);
            if (_overwriteAssessmentData || completedPath == null)
            {
                options.Add(trail.selectionName);
                PathData path = ResourceManager.Instance.LoadPathData(trail.name);
                _experiment.paths.Add(trail, path);
            }
        }
        _pathDropdown.AddOptions(options);
        _buttonStartExperiment.interactable = true;
        OnPathSelectionChanged(0);
    }

    // Called on button press
    public void NewExperiment()
    {
        _experimentID = DataManager.Instance.Settings.CompletedExperiments;
        _inputExperimentID.text = _experimentID.ToString();

        PopulatePathOptions();
    }

}