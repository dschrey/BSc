using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Omnifinity.Omnideck;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class UINewExperiment : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputExperimentID;
    [SerializeField] private Button _buttonEditExperimentID;
    [SerializeField] private TMP_Dropdown _pathDropdown;
    [SerializeField] private TMP_InputField _inputParticipantName;
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
    private bool _omnideckConnected;

    [SerializeField] public InputActionReference StartAction;


    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _inputExperimentID.onEndEdit.AddListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.AddListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.AddListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.AddListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.AddListener(OnResetExperimentCount);
        _toggleResetAssessmentData.onValueChanged.AddListener(OnResetAssessmentData);
        _buttonOmnideckStatus.onClick.AddListener(OnOmnideckStatusChanged);
    }


    private void OnDisable()
    {
        _inputExperimentID.onEndEdit.RemoveListener(OnExperimentIDChanged);
        _buttonEditExperimentID.onClick.RemoveListener(OnEditExperimentIDClicked);
        _buttonStartExperiment.onClick.RemoveListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.RemoveListener(OnPathSelectionChanged);
        _toggleResetExperimentCount.onValueChanged.RemoveListener(OnResetExperimentCount);
        _toggleResetAssessmentData.onValueChanged.RemoveListener(OnResetAssessmentData);
        _buttonOmnideckStatus.onClick.RemoveListener(OnOmnideckStatusChanged);
    }

    void Start()
    {
        _omnideckConnected = true;
        _experimentID = DataManager.Instance.Settings.CompletedExperiments;
        _inputExperimentID.text = _experimentID.ToString();
        _currentAssessment = DataManager.Instance.TryLoadAssessmentData(_experimentID);
        if (_currentAssessment == null) _toggleResetAssessmentData.isOn = false;
        _toggleResetAssessmentData.interactable = _currentAssessment != null;
        PopulatePathOptions();

        // TODO Remove after testing
        //StartCoroutine(StartScene());
    }

    private void Update()
    {
        if (StartAction != null && StartAction.action.WasPressedThisFrame())
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

        _buttonStartExperiment.interactable = CheckStartRequirements();
        Debug.Log($"Experiment {_experiment.id} selected Path: {_selectedPath.PathID} - {_selectedPath.name}");
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

    private void OnOmnideckStatusChanged()
    {
        return;
        Debug.Log($"Omnideck State : {_omnideckConnected}");

        
        if (!_omnideckConnected)
        {
            _omnideckConnected = !_omnideckConnected;

            if (!Instantiate(_omnideckInterfacePrefab, _interfaceParent))
            {
                Debug.LogError("Could not find Omnideck Interface.");
                _omnideckConnected = !_omnideckConnected;
                _textOmnideckStatus.color = Color.yellow;
                _textOmnideckStatus.text = "Connection failed";
                _textOmnideckStatusButton.color = Color.green;
                _textOmnideckStatusButton.text = "Enable";
                return;
            }
            _textOmnideckStatus.color = Color.green;
            _textOmnideckStatus.text = "Enabled";
            _textOmnideckStatusButton.color = Color.red;
            _textOmnideckStatusButton.text = "Disable";
        }
        else
        {
            _omnideckConnected = !_omnideckConnected;
            Destroy(FindObjectOfType<OmnideckInterface>().gameObject);
            _textOmnideckStatus.color = Color.red;
            _textOmnideckStatus.text = "Disabled";
            _textOmnideckStatusButton.color = Color.green;
            _textOmnideckStatusButton.text = "Enable";
        }

        //
        //if (_omnideckConnected && omnideckInterface.GetTreadmillStatus() == ETreadmillStatus.Stopped)
        //{
        //    _omnideckConnected = !_omnideckConnected;
        //    omnideckInterface.enabled = _omnideckConnected;
        //    _textOmnideckStatus.color = Color.yellow;
        //    _textOmnideckStatus.text = "Unable to connect";
        //    return;
        //}

        XROrigin XROrigin = FindObjectOfType<XROrigin>();
        if (XROrigin == null)
        {
            Debug.LogError("Could not find XROrigin.");
            return;
        }

        OmnideckContinuousMove omnideckMove = XROrigin.GetComponentInChildren<OmnideckContinuousMove>();
        if (omnideckMove == null)
        {
            Debug.LogError("Could not find Omnideck Continuous Move provider.");
            return;
        }
        omnideckMove.enabled = _omnideckConnected;


        _buttonStartExperiment.interactable = CheckStartRequirements();
    }

    
    private bool CheckStartRequirements()
    {
        if (_selectedPath == null) return false;
        //if (_selectedTrail.floor == FloorType.OMNIDECK && !_omnideckConnected) return false;
        return true;
    }

    private IEnumerator StartScene()
    {
        yield return new WaitForSeconds(5);
        OnStartExperimentClicked();
    }

}