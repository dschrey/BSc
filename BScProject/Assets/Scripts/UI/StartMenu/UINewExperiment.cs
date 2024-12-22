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

    [SerializeField] private TMP_InputField _inputPerticipantName;
    [SerializeField] private Button _buttonConfigureStartPosition;
    [SerializeField] private Button _buttonStartExperiment;

    private PathData _selectedPath;
    private int _experimentID;
    private string _participantName;
   
   
   // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnEnable()
    {
        _inputExperimentID.onEndEdit.AddListener(OnExperimentIDChanged);
        _inputPerticipantName.onEndEdit.AddListener(OnParticipantNameChanged);
        _buttonEditExperimentID.onClick.AddListener(OnEditExperimentIDClicked);
        _buttonConfigureStartPosition.onClick.AddListener(OnConfigureStartPositionClicked);
        _buttonStartExperiment.onClick.AddListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.AddListener(OnPathSelectionChanged);
    }

    private void OnDisable() 
    {
        _inputExperimentID.onEndEdit.RemoveListener(OnExperimentIDChanged);
        _inputPerticipantName.onEndEdit.RemoveListener(OnParticipantNameChanged);
        _buttonEditExperimentID.onClick.RemoveListener(OnEditExperimentIDClicked);
        _buttonConfigureStartPosition.onClick.RemoveListener(OnConfigureStartPositionClicked);
        _buttonStartExperiment.onClick.RemoveListener(OnStartExperimentClicked);
        _pathDropdown.onValueChanged.RemoveListener(OnPathSelectionChanged);
    }

    void Start()
    {
        _experimentID = DataManager.Instance.ExperimentData.CompletedAssessments;
        _inputExperimentID.text = _experimentID.ToString();

        List<string> options = new();
        ResourceManager.Instance.LoadedPaths.ForEach(p =>
        {
            options.Add(p.name);
        });
        _pathDropdown.AddOptions(options);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExperimentIDChanged(string input)
    {
        int.TryParse(input, out int value);
        _experimentID = value;
    }

    private void OnEditExperimentIDClicked()
    {
        bool enabled = _inputExperimentID.interactable;
        _inputExperimentID.interactable = ! enabled;
    }

    private void OnParticipantNameChanged(string input)
    {
        _participantName = input;
    }

    private void OnConfigureStartPositionClicked()
    {
        throw new NotImplementedException();
    }

    private void OnStartExperimentClicked()
    {
        AssessmentManager.Instance.CreateNewAssessment(_experimentID, _participantName, _selectedPath);
    }

    private void OnPathSelectionChanged(int index)
    {
        _selectedPath = ResourceManager.Instance.LoadedPaths[index];
        Debug.Log($"Selected Path: {ResourceManager.Instance.LoadedPaths[index].name}");
    }

}
