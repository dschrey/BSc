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

    [SerializeField] private Button _buttonExpand;
    [SerializeField] private Button _buttonMinimize;
    [SerializeField] private Button _buttonSegmentHint;
    [SerializeField] private Button _buttonRestartSegment;
    [SerializeField] private Button _buttonStopExperiment;
    [SerializeField] private Button _buttonConfirmStop;
    [SerializeField] private Button _buttonCancelStop;

    [SerializeField] private GameObject _infoPanelExpanded;
    [SerializeField] private GameObject _infoPanelMinimized;



    private bool _infoShown;
    private Timer _timer;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Update() 
    {
        if (_infoShown && _timer != null)
        {
            _textTime.text = _timer.GetElapsedTimeFormated();
            _textStatus.text = GetCurrentStatus();
            _textID.text = ExperimentManager.Instance.CurrentExperiment.id.ToString();
            _textSegmentID.text = PathManager.Instance.CurrentSegment.PathSegmentData.SegmentID.ToString();
        }
    }

    void OnEnable()
    {
        _infoShown = false;
        _buttonExpand.onClick.AddListener(OnExpandExpermintInfo);
        _buttonMinimize.onClick.AddListener(OnMinimizeExpermintInfo);

        _buttonSegmentHint.onClick.AddListener(OnShowObjectiveHintButtonClicked);
        _buttonRestartSegment.onClick.AddListener(OnRestartSegmentButtonClicked);
        _buttonStopExperiment.onClick.AddListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.AddListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.AddListener(OnCancelStopExperimentClicked);
    }  

    void OnDisable()
    {
        _buttonSegmentHint.onClick.RemoveListener(OnShowObjectiveHintButtonClicked);
        _buttonRestartSegment.onClick.RemoveListener(OnRestartSegmentButtonClicked);
        _buttonStopExperiment.onClick.RemoveListener(OnStopExperimentClicked);
        _buttonConfirmStop.onClick.RemoveListener(OnConfirmStopExperimentClicked);
        _buttonCancelStop.onClick.RemoveListener(OnCancelStopExperimentClicked);
    }

    // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------

    private void OnExpandExpermintInfo()
    {
        _timer = ExperimentManager.Instance.Timer;
        _infoShown = true;
        _infoPanelMinimized.SetActive(!_infoShown);
        _infoPanelExpanded.SetActive(_infoShown);

        _textPathName.text = PathManager.Instance.CurrentPath.PathData.Name;
    }

    private void OnMinimizeExpermintInfo()
    {
        _infoShown = false;
        _infoPanelExpanded.SetActive(_infoShown);   
        _infoPanelMinimized.SetActive(! _infoShown);
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

    private string GetCurrentStatus()
    {
        switch (ExperimentManager.Instance.ExperimentState)
        {
            case ExperimentState.IDLE:
                return "IDLE";
            case ExperimentState.RUNNING:
                return "RUNNING";
            case ExperimentState.ASSESSMENT:
                return "ASSESSMENT";
            case ExperimentState.FINISHED:
                return "FINISHED";
        }

        return "ERROR";
    }

}
