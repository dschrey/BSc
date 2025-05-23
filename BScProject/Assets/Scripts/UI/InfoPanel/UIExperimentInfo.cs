using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExperimentInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _textStatus;
    [SerializeField] private TMP_Text _textPathName;
    [SerializeField] private TMP_Text _textID;
    [SerializeField] private TMP_Text _textFloorType;
    [SerializeField] private TMP_Text _textNumHints;
    [SerializeField] private TMP_Text _textTime;
    [SerializeField] private TMP_Text _textAssessmentStage;
    [SerializeField] private TMP_Text _textSegmentID;

    [SerializeField] private Button _buttonExpand;
    [SerializeField] private Button _buttonMinimize;
    [SerializeField] private Button _buttonSegmentHint;
    [SerializeField] private Button _buttonStopExperiment;
    [SerializeField] private Button _buttonConfirmStop;
    [SerializeField] private Button _buttonCancelStop;
    [SerializeField] private Button _buttonAssessmentStart;

    [Header("Panel Sizes"), SerializeField] private GameObject _infoPanelExpanded;
    [SerializeField] private GameObject _infoPanelMinimized;
    [Header("Stage Panels"), SerializeField] private GameObject _segmentSection;
    [SerializeField] private GameObject _assessmentSection;

    private bool _assessmentInfoShown = false;

    public bool IsPanelShown;
    private Timer _timer;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (IsPanelShown && _timer != null)
        {
            _textTime.text = _timer.GetTimeFormated();
            _textStatus.text = ExperimentManager.Instance.ExperimentState.ToString();
            _textNumHints.text = PathManager.Instance.HintCounter.ToString();
            if (!_assessmentInfoShown)
            {
                _textSegmentID.text = PathManager.Instance.CurrentSegment.PathSegmentData.SegmentID.ToString();
            }
            else
            {
                _textAssessmentStage.text = AssessmentManager.Instance.AssessmentStep.ToString();
            }
        }
    }

    void OnEnable()
    {
        IsPanelShown = false;
        _buttonExpand.onClick.AddListener(OnExpandExpermintInfo);
        _buttonMinimize.onClick.AddListener(OnMinimizeExpermintInfo);

        _buttonSegmentHint.onClick.AddListener(OnShowObjectiveHintButtonClicked);
        _buttonStopExperiment.onClick.AddListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.AddListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.AddListener(OnCancelStopExperimentClicked);
        _buttonAssessmentStart.onClick.AddListener(OnAssessmentStartPressed);
    }

    void OnDisable()
    {
        _buttonSegmentHint.onClick.RemoveListener(OnShowObjectiveHintButtonClicked);
        _buttonStopExperiment.onClick.RemoveListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.RemoveListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.RemoveListener(OnCancelStopExperimentClicked);
        _buttonAssessmentStart.onClick.RemoveListener(OnAssessmentStartPressed);
    }

    private void OnAssessmentStartPressed()
    {

        ExperimentManager.Instance.StartAssessment();
        _buttonAssessmentStart.interactable = false;
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExpandExpermintInfo()
    {
        _timer = StudyManager.Instance.TrialTimer;
        IsPanelShown = true;
        _infoPanelMinimized.SetActive(!IsPanelShown);
        _infoPanelExpanded.SetActive(IsPanelShown);

        _textPathName.text = StudyManager.Instance.StudyData.TrialPath.name;
        _textFloorType.text = StudyManager.Instance.StudyData.LocomotionMethod.ToString();
        _textID.text = DataManager.Instance.StudyData.ParticipantNumber.ToString();
    }

    private void OnMinimizeExpermintInfo()
    {
        IsPanelShown = false;
        _infoPanelExpanded.SetActive(IsPanelShown);
        _infoPanelMinimized.SetActive(!IsPanelShown);
    }

    private void OnShowObjectiveHintButtonClicked()
    {
        PathManager.Instance.DisplaySegmentHint();
    }

    private void OnStopExperimentClicked()
    {
        _buttonStopExperiment.gameObject.SetActive(false);
        _buttonConfirmStop.gameObject.SetActive(true);
        _buttonCancelStop.gameObject.SetActive(true);
    }

    private void OnConfirmStopExperimentClicked()
    {
        ExperimentManager.Instance.StopExperiment();
    }

    private void OnCancelStopExperimentClicked()
    {
        _buttonStopExperiment.gameObject.SetActive(true);
        _buttonConfirmStop.gameObject.SetActive(false);
        _buttonCancelStop.gameObject.SetActive(false);
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------


    public void ToggleAssessmentControl()
    {
        _assessmentInfoShown = true;
        _segmentSection.SetActive(false);
        _assessmentSection.SetActive(true);
        _textAssessmentStage.text = AssessmentManager.Instance.AssessmentStep.ToString();
    }

}
