using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExperimentInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _textStatus;
    [SerializeField] private TMP_Text _textTime;
    [SerializeField] private TMP_Text _textID;
    [SerializeField] private TMP_Text _textSegmentID;
    [SerializeField] private TMP_Text _textPathName;
    [SerializeField] private TMP_Text _textAssessmentStage;

    [SerializeField] private Button _buttonExpand;
    [SerializeField] private Button _buttonMinimize;
    [SerializeField] private Button _buttonSegmentHint;
    [SerializeField] private Button _buttonRestartSegment;
    [SerializeField] private Button _buttonStopExperiment;
    [SerializeField] private Button _buttonConfirmStop;
    [SerializeField] private Button _buttonCancelStop;
    [SerializeField] private Button _buttonAssessmentControl;

    [SerializeField] private GameObject _infoPanelExpanded;
    [SerializeField] private GameObject _infoPanelMinimized;

    [SerializeField] private GameObject _segmentSection;
    [SerializeField] private GameObject _assessmentSection;

    private bool _assessmentInfoShown = false;

    public bool IsPanelShown;
    private Timer _timer;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (IsPanelShown && _timer != null)
        {
            _textTime.text = _timer.GetElapsedTimeFormated();
            _textStatus.text = GetExerpimentStatus();
            _textID.text = ExperimentManager.Instance.CurrentExperiment.id.ToString();
            if (!_assessmentInfoShown)
            {
                _textSegmentID.text = PathManager.Instance.CurrentSegment.PathSegmentData.SegmentID.ToString();
            }
            else
            {
                _textAssessmentStage.text = GetAssessmentStatus();
            }
        }
    }

    void OnEnable()
    {
        IsPanelShown = false;
        _buttonExpand.onClick.AddListener(OnExpandExpermintInfo);
        _buttonMinimize.onClick.AddListener(OnMinimizeExpermintInfo);

        _buttonSegmentHint.onClick.AddListener(OnShowObjectiveHintButtonClicked);
        _buttonRestartSegment.onClick.AddListener(OnRestartSegmentButtonClicked);
        _buttonStopExperiment.onClick.AddListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.AddListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.AddListener(OnCancelStopExperimentClicked);
        _buttonAssessmentControl.onClick.AddListener(OnAssessmentStartPressed);
    }

    void OnDisable()
    {
        _buttonSegmentHint.onClick.RemoveListener(OnShowObjectiveHintButtonClicked);
        _buttonRestartSegment.onClick.RemoveListener(OnRestartSegmentButtonClicked);
        _buttonStopExperiment.onClick.RemoveListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.RemoveListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.RemoveListener(OnCancelStopExperimentClicked);
        _buttonAssessmentControl.onClick.RemoveListener(OnAssessmentStartPressed);
    }

    private void OnAssessmentStartPressed()
    {

        ExperimentManager.Instance.StartAssessment();
        _buttonAssessmentControl.interactable = false;
        //AssessmentManager.Instance.GoToPreviousAssessmentStep();
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExpandExpermintInfo()
    {
        _timer = ExperimentManager.Instance.Timer;
        IsPanelShown = true;
        _infoPanelMinimized.SetActive(!IsPanelShown);
        _infoPanelExpanded.SetActive(IsPanelShown);

        _textPathName.text = PathManager.Instance.CurrentPath.PathData.PathName;
    }

    private void OnMinimizeExpermintInfo()
    {
        IsPanelShown = false;
        _infoPanelExpanded.SetActive(IsPanelShown);
        _infoPanelMinimized.SetActive(!IsPanelShown);
    }

    private void OnRestartSegmentButtonClicked()
    {
        PathManager.Instance.RestartSegment();
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

    private string GetExerpimentStatus()
    {
        switch (ExperimentManager.Instance.ExperimentState)
        {
            case ExperimentState.Idle:
                return "IDLE";
            case ExperimentState.Running:
                return "RUNNING";
            case ExperimentState.Assessment:
                return "ASSESSMENT";
            case ExperimentState.Finished:
                return "FINISHED";
        }

        return "ERROR";
    }

    public void ToggleAssessmentControl()
    {
        _assessmentInfoShown = true;
        _segmentSection.SetActive(false);
        _assessmentSection.SetActive(true);
        _textAssessmentStage.text = GetAssessmentStatus();
    }

    private string GetAssessmentStatus()
    {
        switch (AssessmentManager.Instance.AssessmentStep)
        {
            case AssessmentStep.Idle:
                return "IDLE";
            case AssessmentStep.PathSelection:
                return "PATHSELECTION";
            case AssessmentStep.SegmentDistance:
                return "SEGMENTDISTANCE";
            case AssessmentStep.HoverObjectSelection:
                return "OBJECTASSIGN";
            case AssessmentStep.LandmarkSelection:
                return "OBJECTPOSITION";
            case AssessmentStep.Completed:
                return "COMPLETED";
        }
        return "ERROR";
    }
}
