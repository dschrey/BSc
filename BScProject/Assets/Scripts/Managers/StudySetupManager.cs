using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StudySetupManager : MonoBehaviour
{
    #region Variables

    public StudyData studyData;

    [Header("UI Elements"), SerializeField]
    private TMP_InputField _inputParticipantNumber;
    [SerializeField]
    private Button _buttonEditExperimentID;
    [SerializeField]
    private Button _buttonReloadStudyData;
    [SerializeField]
    private TMP_Dropdown _dropdownPrimaryHand;
    [SerializeField]
    private TMP_Dropdown _dropdownPathSet;
    [SerializeField]
    private TMP_Dropdown _dropdownBlockNumber;
    [SerializeField]
    private TMP_Dropdown _dropdownTrailInBlock;
    [SerializeField]
    private TMP_Dropdown _dropdownLocomotionMethod;
    [SerializeField]
    private TMP_Dropdown _dropdownTrialPath;

    [SerializeField]
    private TMP_Text _textOverallTrail;
    [SerializeField]
    private Button _buttonStartStudy;
    [SerializeField]
    private Toggle _toggleResetExperimentCount;
    [SerializeField]
    private Toggle _toggleResetAssessmentData;
    [SerializeField]
    private TMP_Text _textInstructions;
    private Trail _selectedTrail;
    private int _experimentID;
    private ExperimentData _experiment;
    private bool _overwriteCompletedExperiment = false;
    private bool _overwriteAssessmentData = false;
    public AssessmentData _currentAssessment = null;


    [SerializeField]
    private UINewExperiment _newExperimentPanel;
    [SerializeField]
    private UIExperimentContinue _continueExperimentPanel;
    [SerializeField]
    private GameObject _finishedExperimentPanel;

    [Header("Instruction"), SerializeField]
    private TMP_Text _instructionText;


    [Header("Debug: Selection Values"), SerializeField]
    public int _selectedParticipantNumber = -1;
    public int _selectedBlockNumber = -1;
    public int _selectedTrialInBock = -1;
    public LocomotionMethod _selectedLocomotionMethod;
    public PrimaryHand _selectedPrimaryHand;
    public PathSet _selectedPathSet;
    public PathData _selectedPath;
    public bool _studyInProgress = false;
    #endregion

    private void OnEnable()
    {
        if (studyData == null)
        {
            Debug.LogError($"StudyData is missing.");
            return;
        }

        _dropdownPrimaryHand.onValueChanged.AddListener(OnPrimaryHandChanged);
        _dropdownPathSet.onValueChanged.AddListener(OnPathSetChanged);
        _dropdownBlockNumber.onValueChanged.AddListener(OnBlockNumberChanged);
        _dropdownTrailInBlock.onValueChanged.AddListener(OnTrialInBlockChanged);
        _dropdownLocomotionMethod.onValueChanged.AddListener(OnLocomotionMethodChanged);
        _dropdownTrialPath.onValueChanged.AddListener(OnTrialPathChanged);

        _buttonReloadStudyData.onClick.AddListener(OnReloadStudyDataClicked);

        _inputParticipantNumber.onEndEdit.AddListener(OnExperimentIDChanged);
        // _buttonEditExperimentID.onClick.AddListener(OnEditExperimentIDClicked);
        _buttonStartStudy.onClick.AddListener(OnStartStudyClicked);
        // _toggleResetExperimentCount.onValueChanged.AddListener(OnResetExperimentCount);
        // _toggleResetAssessmentData.onValueChanged.AddListener(OnResetAssessmentData);
    }



    private void OnDisable()
    {
        _dropdownPrimaryHand.onValueChanged.RemoveListener(OnPrimaryHandChanged);
        _dropdownLocomotionMethod.onValueChanged.RemoveListener(OnLocomotionMethodChanged);
        _dropdownBlockNumber.onValueChanged.RemoveListener(OnBlockNumberChanged);
        _dropdownTrailInBlock.onValueChanged.RemoveListener(OnTrialInBlockChanged);
        _dropdownPathSet.onValueChanged.RemoveListener(OnPathSetChanged);
        _dropdownTrialPath.onValueChanged.RemoveListener(OnTrialPathChanged);

        _buttonReloadStudyData.onClick.RemoveListener(OnReloadStudyDataClicked);
        _inputParticipantNumber.onEndEdit.RemoveListener(OnExperimentIDChanged);
        // _buttonEditExperimentID.onClick.RemoveListener(OnEditExperimentIDClicked);
        _buttonStartStudy.onClick.RemoveListener(OnStartStudyClicked);
        // _toggleResetExperimentCount.onValueChanged.RemoveListener(OnResetExperimentCount);
        // _toggleResetAssessmentData.onValueChanged.RemoveListener(OnResetAssessmentData);
    }

    [Header("Debug"), SerializeField]
    private InputActionReference _debugAction;

    void Start()
    {
        _selectedParticipantNumber = studyData.NumberCompletedParticipants + 1;
        _inputParticipantNumber.text = _selectedParticipantNumber.ToString();
        _textOverallTrail.text = studyData.OverallTrialNumber.ToString();

        PopulateDropDown<LocomotionMethod>(_dropdownLocomotionMethod);
        PopulateDropDown<PrimaryHand>(_dropdownPrimaryHand);
        PopulateDropDown(_dropdownPathSet, ResourceManager.Instance.PathSets);
        if (studyData.PathSet != null)
            PopulateDropDown(_dropdownTrialPath, studyData.PathSet.PathList);
        PopulateDropDown(_dropdownBlockNumber, Enum.GetValues(typeof(LocomotionMethod)).Length);
        PopulateDropDown(_dropdownTrailInBlock, studyData.NumberOfPathsPerBlock);

        // Use the number of completed participants to set up next trail
        PrepareStudyData();
    }

    public void SetPlayerInstruction(string instruction)
    {
        _instructionText.text = instruction;
    }

    private void PrepareStudyData()
    {
        if (DataManager.Instance.ReadLastTrialData(studyData))
        {
            studyData.PrepareNextTrial();
        }
        else
        {
            if (!studyData.PrepareNewParticipant(_selectedParticipantNumber))
            {
                return;
            }
        }

        UpdateMenuFields();
    }

    private void UpdateMenuFields()
    {
        _selectedParticipantNumber = studyData.ParticipantNumber;
        _inputParticipantNumber.text = _selectedParticipantNumber.ToString();
        _textOverallTrail.text = studyData.OverallTrialNumber.ToString();

        _dropdownLocomotionMethod.value = _dropdownLocomotionMethod.options.FindIndex(option => option.text == studyData.LocomotionMethod.ToString());
        _selectedLocomotionMethod = studyData.LocomotionMethod;
        _dropdownPrimaryHand.value = _dropdownPrimaryHand.options.FindIndex(option => option.text == studyData.PrimaryHand.ToString());
        _selectedPrimaryHand = studyData.PrimaryHand;
        _dropdownBlockNumber.value = _dropdownBlockNumber.options.FindIndex(option => option.text == studyData.BlockNumber.ToString());
        _selectedBlockNumber = studyData.BlockNumber;
        _dropdownTrailInBlock.value = _dropdownTrailInBlock.options.FindIndex(option => option.text == studyData.TrialInBlock.ToString());
        _selectedTrialInBock = studyData.TrialInBlock;
        _dropdownPathSet.value = _dropdownPathSet.options.FindIndex(option => option.text == studyData.PathSet.SetName);
        _selectedPathSet = studyData.PathSet;
        PopulateDropDown(_dropdownTrialPath, studyData.PathSet.PathList);
        _dropdownTrialPath.value = _dropdownTrialPath.options.FindIndex(option => option.text == studyData.TrialPath.PathName);
        _selectedPath = studyData.TrialPath;

    }

    private void Update()
    {
        if (_debugAction != null && _debugAction.action.WasPressedThisFrame())
        {
            OnStartStudyClicked();
        }
    }

    public void LoadNextExperimentPanel(ExperimentData experiment, ExperimentState experimentState, AssessmentData assessmentData)
    {

        if (experiment.paths.Count > 0)
        {
            _continueExperimentPanel.gameObject.SetActive(true);
            _newExperimentPanel.gameObject.SetActive(false);
            _continueExperimentPanel.ContinueExperiment(experiment, assessmentData);
            return;
        }

        if (assessmentData == null)
        {
            Debug.LogError($"Assessment data is null..");
            return;
        }
        _newExperimentPanel.gameObject.SetActive(false);
        assessmentData.Completed = true;
        DataManager.Instance.Settings.CompletedExperiments++;
        DataManager.Instance.SaveAssessmentData(assessmentData);
        DataManager.Instance.SaveSettings();
        _finishedExperimentPanel.SetActive(true);
    }

    #region Listeners

    private void OnReloadStudyDataClicked()
    {
        PrepareStudyData();
    }

    private void OnStartStudyClicked()
    {

        studyData.SetStudyData(_selectedBlockNumber, _selectedTrialInBock, _selectedPathSet,
            _selectedPath, _selectedLocomotionMethod, _selectedPrimaryHand);

        StudyManager.Instance.SetupTrial(studyData);
        _studyInProgress = true;
    }

    private void OnExperimentIDChanged(string input)
    {
        if (!int.TryParse(input, out int value)) return;
        _selectedParticipantNumber = value;
        if (studyData.PrepareNewParticipant(_selectedParticipantNumber))
        {
            UpdateMenuFields();
        }
        // _currentAssessment = DataManager.Instance.TryLoadAssessmentData(_experimentID);
        // if (_currentAssessment == null) _toggleResetAssessmentData.isOn = false;
        // _toggleResetAssessmentData.interactable = _currentAssessment != null;
        // PopulatePathOptions();
    }

    // private void OnEditExperimentIDClicked()
    // {
    //     bool enabled = _inputParticipantNumber.interactable;
    //     _toggleResetExperimentCount.gameObject.SetActive(!enabled);
    //     _toggleResetAssessmentData.gameObject.SetActive(!enabled);
    //     _inputParticipantNumber.interactable = !enabled;
    // }

    private void OnPathSetChanged(int index)
    {
        string setName = _dropdownPathSet.options[index].text;
        _selectedPathSet = ResourceManager.Instance.GetPathSet(setName);
    }

    // private void OnResetExperimentCount(bool state)
    // {
    //     _overwriteCompletedExperiment = state;
    // }

    // private void OnResetAssessmentData(bool state)
    // {
    //     _overwriteAssessmentData = state;
    //     _buttonStartStudy.interactable = state;
    //     PopulatePathOptions();
    // }

    private void OnBlockNumberChanged(int index)
    {
        _selectedBlockNumber = index + 1;
    }

    private void OnTrialInBlockChanged(int index)
    {
        _selectedTrialInBock = index + 1;
    }

    private void OnPrimaryHandChanged(int index)
    {
        string primaryHand = _dropdownPrimaryHand.options[index].text;

        if (Enum.TryParse(primaryHand, ignoreCase: false, out PrimaryHand result))
        {
            _selectedPrimaryHand = result;
        }
    }

    private void OnLocomotionMethodChanged(int index)
    {
        string locomotionMethod = _dropdownLocomotionMethod.options[index].text;
        if (Enum.TryParse(locomotionMethod, ignoreCase: false, out LocomotionMethod result))
        {
            _selectedLocomotionMethod = result;
        }
    }

    private void OnTrialPathChanged(int index)
    {
        string pathName = _dropdownPathSet.options[index].text;
        _selectedPath = ResourceManager.Instance.GetPathData(pathName);
    }

    #endregion

    #region Class Method

    public void PopulateDropDown<T>(TMP_Dropdown dropdownObj) where T : Enum
    {
        dropdownObj.options.Clear();
        dropdownObj.options.AddRange(Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(item => new TMP_Dropdown.OptionData(item.ToString())));
        dropdownObj.value = 0;
        dropdownObj.RefreshShownValue();
    }

    public void PopulateDropDown(TMP_Dropdown dropdownObj, int numberCount)
    {
        dropdownObj.options.Clear();
        dropdownObj.options.AddRange(Enumerable.Range(1, numberCount)
            .Select(item => new TMP_Dropdown.OptionData(item.ToString())));
        dropdownObj.value = 0;
        dropdownObj.RefreshShownValue();
    }

    public void PopulateDropDown(TMP_Dropdown dropdownObj, List<PathSet> content)
    {
        dropdownObj.options.Clear();
        content.ForEach(item => dropdownObj.options.Add(new TMP_Dropdown.OptionData(item.SetName)));
        dropdownObj.value = 0;
        dropdownObj.RefreshShownValue();
    }

    public void PopulateDropDown(TMP_Dropdown dropdownObj, List<PathData> content)
    {
        dropdownObj.options.Clear();
        content.ForEach(item => dropdownObj.options.Add(new TMP_Dropdown.OptionData(item.PathName)));
        dropdownObj.value = 0;
        dropdownObj.RefreshShownValue();
    }

    // private void PopulatePathOptions()
    // {
    //     _dropdownPathSet.ClearOptions();
    //     List<string> options = new();
    //     ParticipantData data = ResourceManager.Instance.GetParticipantData(_experimentID);
    //     if (data == null)
    //     {
    //         Debug.LogError($"Failed to load experiment data for id: {_experimentID}.");
    //         options.Add("No path found");
    //         _dropdownPathSet.AddOptions(options);
    //         return;
    //     }

    //     _experiment = new(_experimentID);

    //     if (_currentAssessment != null && !_overwriteAssessmentData)
    //     {
    //         if (_currentAssessment.Completed)
    //         {
    //             options.Add("Completed");
    //             _buttonStartStudy.interactable = false;
    //             _dropdownPathSet.AddOptions(options);
    //             return;
    //         }
    //     }

    //     List<string> completedPaths = new();
    //     _currentAssessment?.Paths.ForEach(path =>
    //     {
    //         string floor = path.FloorType == 0 ? "Reg" : "Omni";
    //         string selectionName = $"{_currentAssessment.AssessmentID}_{path.Name}_{floor}";
    //         completedPaths.Add(selectionName);
    //     });

    // foreach (Trail trail in data.paths)
    // {
    //     string completedPath = completedPaths.Find(name => name == trail.selectionName);
    //     if (_overwriteAssessmentData || completedPath == null)
    //     {
    //         options.Add(trail.selectionName);
    //         PathData path = ResourceManager.Instance.LoadPathData(trail.name);
    //         _experiment.paths.Add(trail, path);
    //     }
    // }
    //     _dropdownPathSet.AddOptions(options);
    //     _buttonStartStudy.interactable = true;
    //     OnPathSetChanged(0);
    // }

    // public void NewExperiment()
    // {
    //     _experimentID = DataManager.Instance.settings.CompletedExperiments;
    //     _inputParticipantNumber.text = _experimentID.ToString();

    //     PopulatePathOptions();
    // }

    #endregion
    

    float Percentile(float[] seq, float p)
    {
        var sorted = seq.OrderBy(x=>x).ToArray();
        float idx = (sorted.Length - 1) * p;
        int lo = (int)Math.Floor(idx), hi = (int)Math.Ceiling(idx);
        return (sorted[lo] + sorted[hi]) * 0.5f;
    }
}
