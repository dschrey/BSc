using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINewExperiment : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputExperimentID;
    [SerializeField] private Button _buttonEditExperimentID;
    [SerializeField] private TMP_Dropdown _pathDropdown;

    [SerializeField] private TMP_InputField _inputParticipantName;
    [SerializeField] private Button _buttonStartExperiment;

    [SerializeField] private Toggle _toggleResetExperimentCount;
    private Trail _selectedTrail;
    private PathData _selectedPath;
    private int _experimentID;
    private string _participantName;
    private ExperimentData _experiment;

    private bool _overwriteCompletedExperiments = false;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _inputExperimentID.onEndEdit.AddListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.AddListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.AddListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.AddListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.AddListener(OnResetExperimentCount);
    }

    private void OnDisable()
    {
        _inputExperimentID.onEndEdit.RemoveListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.RemoveListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.RemoveListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.RemoveListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.RemoveListener(OnResetExperimentCount);
    }

    void Start()
    {
        _experimentID = DataManager.Instance.Settings.CompletedExperiments;
        _inputExperimentID.text = _experimentID.ToString();
        PopulatePathOptions();
        OnPathSelectionChanged(0);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExperimentIDChanged(string input)
    {
        int.TryParse(input, out int value);
        _experimentID = value;
        PopulatePathOptions();
        OnPathSelectionChanged(0);
    }

    private void OnEditExperimentIDClicked()
    {
        _toggleResetExperimentCount.gameObject.SetActive(true);
        bool enabled = _inputExperimentID.interactable;
        _inputExperimentID.interactable = !enabled;
    }

    private void OnStartExperimentClicked()
    {
        if (_overwriteCompletedExperiments)
        {
            DataManager.Instance.Settings.CompletedExperiments = _experimentID;
            DataManager.Instance.SaveSettings();
        }

        _experiment.paths.Remove(_selectedTrail);
        SceneManager.Instance.LoadExperimentScene(_experiment, _selectedPath, _selectedTrail, new(_experimentID, DateTime.Now));
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

    private void OnResetExperimentCount(bool state)
    {
        _overwriteCompletedExperiments = state;
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

        foreach (Trail trail in data.paths)
        {
            options.Add(trail.selectionName);
            PathData path = ResourceManager.Instance.LoadPathData(trail.name);
            _experiment.paths.Add(trail, path);
        }
        _pathDropdown.AddOptions(options);
    }

    // Called on button press
    public void NewExperiment()
    {
        _experimentID = DataManager.Instance.Settings.CompletedExperiments;
        _inputExperimentID.text = _experimentID.ToString();

        PopulatePathOptions();
        OnPathSelectionChanged(0);
    }

}