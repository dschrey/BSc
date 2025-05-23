using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public enum ExportEvent
{
    NONE,
    NAVIGATION,
    HINT,
    SEGMENT_COMPLETED,
    BACKTRACK,
    BACKTRACK_FINISHED
}

public class DataManager : MonoBehaviour
{

    #region Variables
    public static DataManager Instance { get; private set; }
    public StudySettings Settings;
    public StudyData StudyData;

    [SerializeField]
    private float _dataFlushInterval = 10f;
    [SerializeField]
    private string _dataDecimalPlaces = "F5";
    private string _experimentSettingsPath;
    private string _experimentResultsPath;
    private readonly string DATA_PATH = Application.dataPath + "/Participants";
    private string _baseParticipantData = "";
    private readonly string BASE_FILE_HEADER = "Participant,Overall_Trial,Block,Trial_In_Block,Path_Set,Path_Name,Path_Difficulty,"
                                                + "Locomotion_Method,Primary_Hand";
    private readonly string LOG_FILE_NAME = "Participant_Log.csv";
    private readonly string LOG_FILE_HEADER = "Navigation_time(s),Backtrack_time(s),Assessment_time(s),Trial_Time(s),Study_Time(s),Study_Date,System_Time";
    private readonly string MOVEMENT_FILE_NAME = "_MovementData.csv";
    private readonly string MOVEMENT_FILE_HEADER = "Event_Type,Pos_X,Pos_Y,Pos_Z,Yaw,Pitch,Roll,Segment_Number,Reference_Pos_X,Reference_Pos_Y,Reference_Pos_Z," +
        "Hint_True_Bearing_To_Reference(deg),Hint_Estimated_Bearing_To_Reference(deg),Hint_Signed_Bearing_Error_To_Reference(deg),Hint_Absolute_Bearing_Error_To_Reference(deg),"+
        "Absolute_Distance_To_Reference(m),Date_Time,Elapsed_Trial_Time(s)";
    private readonly string BACKTRACK_TASK_FILE_NAME = "BacktrackData.csv";
    private readonly string BACKTRACK_TASK_HEADER = "Backtrack_Time(s),Selected_Origin_Distance(m),Actual_Origin_Distance(m)," +
        "Signed_Origin_Distance_Error(m),Absolute_Origin_Distance_Error(m),Absolute_Offset_Actual_Origin(m),Signed_Origin_Bearing_Error(deg),Absolute_Origin_Bearing_Error(deg)";
    private readonly string TRIAL_ASSESSMENT_FILE_NAME = "TrialAssessmentData.csv";
    private readonly string TRIAL_ASSESSMENT_HEADER = "Selected_Path_Layout,Actual_Path_Layout,Corrent_Path_Layout,Segment_Number,Segment_Time(s)," +
        "Number_Hints,First_Hint_Time(s),Selected_Turn_Angle_To_Segment(deg),Actual_Turn_Angle_To_Segment(deg)," +
        "Selected_Segment_Distance(m),Actual_Segment_Distance(m),Signed_Segment_Distance_Error(m),Absolute_Segment_Distance_Error(m)," +
        "Selected_Landmark_Object,Actual_Landmark_Object,Corrent_Landmark_Object,True_Bearing_To_Landmark(deg),Estimated_Bearing_To_Landmark(deg)," +
        "Signed_Bearing_Error_To_Landmark(deg),Absolute_Bearing_Error_To_Landmark(deg),Selected_Landmark_Distance(m),Actual_Landmark_Distance(m)," +
        "Signed_Landmark_Distance_Error(m),Absolute_Landmark_Distance_Error(m),Signed_Offset_Actual_Landmark(m),Absolute_Offset_Actual_Landmark(m)";
    private CSVUtils _CSVUtils = new();
    [SerializeField]
    private float MovementSamplingIntervalSec = 0.5f;
    private float _samplingTimer = 0;
    private bool _isRecordingTrackedObject;
    
    [SerializeField] // TODO remove
    private Transform _trackedObjectTransform;
    
    [SerializeField] // TODO remove
    private Transform _trackedReferenceTranform;
    [SerializeField] // TODO remove
    private ExportEvent _exportEvent = ExportEvent.NONE;

    #endregion
    #region Unity Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _experimentSettingsPath = Application.persistentDataPath + "/Settings.json";
        _experimentResultsPath = Application.persistentDataPath + "/Assessments";
    }

    private void Start()
    {
        if (Settings == null)
        {
            Debug.LogError("Study settings object not provided.");
            return;
        }
        if (StudyData == null)
        {
            Debug.LogError("Study data object not provided.");
            return;
        }

        if (!Directory.Exists(_experimentResultsPath))
        {
            Directory.CreateDirectory(_experimentResultsPath);
        }

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        LoadSettings();
        InvokeRepeating("FlushBuffers", _dataFlushInterval, _dataFlushInterval);

        _isRecordingTrackedObject = false;
    }

    void Update()
    {
        if (!_isRecordingTrackedObject) return;

        _samplingTimer += Time.deltaTime;
        if (_samplingTimer >= MovementSamplingIntervalSec)
        {
            ExportMovementData();
            _samplingTimer = 0f;
        }

    }


    private void OnApplicationQuit()
    {
        _CSVUtils.FlushAllBuffers();
    }

    #endregion
    #region Class Methods

    private void FlushBuffers()
    {
        _CSVUtils.FlushAllBuffers();
    }

    public void SaveSettings()
    {
        ExperimentSettingsData data = new()
        {
            CompletedAssessments = Settings.CompletedExperiments,
            PlayerDetectionRadius = Settings.PlayerDetectionRadius,
            ObjectiveRevealTime = Settings.ObjectiveRevealTime,
            MovementSpeedMultiplier = Settings.MovementSpeedMultiplier,
            TransitionDuration = Settings.TransitionDuration
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_experimentSettingsPath, json);
    }


    public void LoadSettings()
    {
        if (File.Exists(_experimentSettingsPath))
        {
            string json = File.ReadAllText(_experimentSettingsPath);

            ExperimentSettingsData data = JsonUtility.FromJson<ExperimentSettingsData>(json);

            Settings.PlayerDetectionRadius = data.PlayerDetectionRadius;
            Settings.ObjectiveRevealTime = data.ObjectiveRevealTime;
            Settings.MovementSpeedMultiplier = data.MovementSpeedMultiplier;
            Settings.CompletedExperiments = data.CompletedAssessments;
            Settings.TransitionDuration = data.TransitionDuration;
        }
        else
        {
            SaveSettings();
        }
    }

    public void SaveAssessmentData(AssessmentData data)
    {
        string jsonData = JsonUtility.ToJson(data, true);

        string filePath = _experimentResultsPath + "/" + $"Assessment_{data.AssessmentID}.json";
        File.WriteAllText(filePath, jsonData);

        Debug.Log($"Assessment data saved to: {filePath}");
    }

    public AssessmentData TryLoadAssessmentData(int assessmentID)
    {
        string filePath = _experimentResultsPath + "/" + $"Assessment_{assessmentID}.json";

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            Debug.Log($"Found exisiting assessment data for ID {assessmentID}");
            return JsonUtility.FromJson<AssessmentData>(jsonData);
        }
        Debug.Log($"No exisiting assessment data for ID {assessmentID}");
        return null;
    }

    public bool ReadLastTrialData(StudyData studyData)
    {
        string overviewFilePath = DATA_PATH + "/" + LOG_FILE_NAME;
        List<string> lines = _CSVUtils.ReadDataFromFile(overviewFilePath);
        if (lines.Count == 0)
        {
            return false;
        }
        var data_values = lines.Last().Split(',');

        studyData.LoadStudyData(data_values[0].ToString(), data_values[1].ToString(), // participantNumber, trialNumber
                                data_values[2].ToString(), data_values[3].ToString(), // blockNumber, trialInBlock
                                data_values[4].ToString(), data_values[5].ToString(), // pathSetName, pathName
                                data_values[6].ToString(), data_values[7].ToString(), // pathDifficulty, locomotionMethod
                                data_values[8].ToString(), data_values[9].ToString(), // primaryHand, navigationTimeSec
                                data_values[10].ToString(), data_values[11].ToString(), // backtrackTimeSec, assessmentTimeSec
                                data_values[12].ToString(), data_values[13].ToString(), // trialTimeSec, studyTimeSec
                                data_values[14].ToString(), data_values[15].ToString()); // studyDate, systemTime
        return true;
    }

    public void SetTrackedObject(Transform trackedTransform)
    {
        _trackedObjectTransform = trackedTransform;
        if (_trackedObjectTransform == null)
        {
            _isRecordingTrackedObject = false;
        }
    }

    public void SetTrackingReferenceObject(Transform referenceObj)
    {
        _trackedReferenceTranform = referenceObj;
        if (_trackedReferenceTranform == null)
        {
            _isRecordingTrackedObject = false;
        }
    }

    public void ToggleObjectTracking(bool state) => _isRecordingTrackedObject = state;

    public void SetExportEvent(ExportEvent exportEvent)
    {
        _exportEvent = exportEvent;
    }

    private float GetAbsoluteDistanceToReference()
    {
        if (_trackedObjectTransform == null)
        {
            Debug.LogError($"Tracked object is null.");
            return -1;
        }
        if (_trackedReferenceTranform == null)
        {
            Debug.LogError($"Tracked reference object is null.");
            return -1;
        }

        Vector3 objectPos = _trackedObjectTransform.position;
        objectPos.y = 0;
        Vector3 refObjectPos = _trackedReferenceTranform.position;
        refObjectPos.y = 0;
        return Vector3.Distance(objectPos, refObjectPos);
    }

    public void SetBaseParticipantData(StudyData studyData)
    {
        _baseParticipantData = studyData.ParticipantNumber.ToString() + "," +
                    studyData.OverallTrialNumber.ToString() + "," +
                    studyData.BlockNumber.ToString() + "," +
                    studyData.TrialInBlock.ToString() + "," +
                    studyData.PathSet.SetName + "," +
                    studyData.TrialPath.PathName + "," +
                    studyData.TrialPath.PathDifficulty.ToString() + "," +
                    studyData.LocomotionMethod.ToString() + "," +
                    studyData.PrimaryHand.ToString();
    }

    #endregion
    #region Export Methods

    public void ExportMovementData(ExportEvent event_type = ExportEvent.NONE)
    {
        ExportEvent eventType = event_type;
        float trueBearingToReference = 0f;
        float estimatedBearingToReference = 0f;
        float signedBearingErrorToReference = 0f;
        float absoluteBearingErrorToReference = 0f;
        if (event_type == ExportEvent.NONE)
        {
            eventType = _exportEvent;
        }
        else if (event_type == ExportEvent.HINT)
        {
            Transform lastSegment = PathManager.Instance.GetLastSegment();

            Vector3 trueMovementDirection = _trackedReferenceTranform.position - lastSegment.position;
            trueBearingToReference = Mathf.Atan2(trueMovementDirection.x, trueMovementDirection.z) * Mathf.Rad2Deg;
            trueBearingToReference = (trueBearingToReference + 360f) % 360f;

            Vector3 estimatedMovementDirection = _trackedObjectTransform.position - lastSegment.position;
            estimatedBearingToReference = Mathf.Atan2(estimatedMovementDirection.x, estimatedMovementDirection.z) * Mathf.Rad2Deg;
            estimatedBearingToReference = (estimatedBearingToReference + 360f) % 360f;

            signedBearingErrorToReference = Mathf.DeltaAngle(trueBearingToReference, estimatedBearingToReference);
            absoluteBearingErrorToReference = Mathf.Abs(signedBearingErrorToReference);
        }
        int segmentNumber = PathManager.Instance.GetCurrentSegmentID();
        string baseFilePath = DATA_PATH;

        if (!Directory.Exists(baseFilePath))
        {
            Directory.CreateDirectory(baseFilePath);
        }

        string outputFilePath = baseFilePath + "/" + "P" + StudyData.ParticipantNumber + MOVEMENT_FILE_NAME;

        string dataToExport = _baseParticipantData + "," +  eventType.ToString()  +  "," + _trackedObjectTransform.position.x.ToString(_dataDecimalPlaces) + "," +
            _trackedObjectTransform.position.y.ToString(_dataDecimalPlaces) + "," + _trackedObjectTransform.position.z.ToString(_dataDecimalPlaces) + "," +
            _trackedObjectTransform.eulerAngles.x.ToString(_dataDecimalPlaces) + "," +_trackedObjectTransform.eulerAngles.y.ToString(_dataDecimalPlaces) + "," +
            _trackedObjectTransform.eulerAngles.z.ToString(_dataDecimalPlaces) + "," + segmentNumber + "," + _trackedReferenceTranform.position.x.ToString(_dataDecimalPlaces) + "," +
            _trackedReferenceTranform.position.y.ToString(_dataDecimalPlaces) + "," + _trackedReferenceTranform.position.z.ToString(_dataDecimalPlaces) + "," +
            trueBearingToReference.ToString(_dataDecimalPlaces) + "," + estimatedBearingToReference.ToString(_dataDecimalPlaces) + "," +
            signedBearingErrorToReference.ToString(_dataDecimalPlaces) + "," + absoluteBearingErrorToReference.ToString(_dataDecimalPlaces) + "," +
            GetAbsoluteDistanceToReference().ToString(_dataDecimalPlaces) + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," +
            StudyManager.Instance.TrialTimer.GetTime();

        string header = BASE_FILE_HEADER + "," + MOVEMENT_FILE_HEADER;
        _CSVUtils.WriteDataToFile(dataToExport, outputFilePath, header);

    }

    public void AddTrialLog(float navigationTimeSec, float backtrackTimeSec, float assessmentTimeSec, float trialTimeSec)
    {
        string baseFilePath = DATA_PATH;

        if (!Directory.Exists(baseFilePath))
        {
            Directory.CreateDirectory(baseFilePath);
        }

        string outputFilePath = baseFilePath + "/" + LOG_FILE_NAME;
        string dataToExport = _baseParticipantData  + "," + navigationTimeSec + "," + backtrackTimeSec + "," +
            assessmentTimeSec + "," + trialTimeSec + "," + (StudyData.TotalStudyTimeSec + trialTimeSec) + "," +
            DateTime.Now.ToString("yyyy-MM-dd") + "," + DateTime.Now.ToString("HH:mm:ss");

        string header = BASE_FILE_HEADER + "," + LOG_FILE_HEADER;
        _CSVUtils.WriteDataToFile(dataToExport, outputFilePath, header);
    }

    public void ExportBacktrackData(float backtrackTimeSec, float selectedOriginDistance, float actualOriginDistance,
                                    float signedOriginDistanceError, float absoluteOriginDistanceError, float absoluteOriginOffest,
                                    float signedBearingErrorToOrigin, float absoluteBearingErrorToOrigin)
    {
        string baseFilePath = DATA_PATH;

        if (!Directory.Exists(baseFilePath))
        {
            Directory.CreateDirectory(baseFilePath);
        }

        string outputFilePath = baseFilePath + "/" + BACKTRACK_TASK_FILE_NAME;
        string dataToExport = _baseParticipantData + "," + backtrackTimeSec + "," + selectedOriginDistance.ToString(_dataDecimalPlaces)
                + "," + actualOriginDistance.ToString(_dataDecimalPlaces) + "," + signedOriginDistanceError.ToString(_dataDecimalPlaces)
                + "," + absoluteOriginDistanceError.ToString(_dataDecimalPlaces) + "," + absoluteOriginOffest.ToString(_dataDecimalPlaces)
                + "," + signedBearingErrorToOrigin.ToString(_dataDecimalPlaces) + "," + absoluteBearingErrorToOrigin.ToString(_dataDecimalPlaces);

        string header = BASE_FILE_HEADER + "," + BACKTRACK_TASK_HEADER;
        _CSVUtils.WriteDataToFile(dataToExport, outputFilePath, header);
    }

    public void ExportAssessmentData(int selectedPathLayout, int actualPathLayout, bool correntPathLayout,
        int segmentNumber, float segmentTime, float numberHints, float firstHintTimeSec, float selectedTurnAngleToSegment, float actualTurnAngleToSegment,
        float selectedSegmentDistance, float actualSegmentDistance, float signedSegmentDistanceError, float absoluteSegmentDistanceError, string selectedLandmarkObject,
        string actualLandmarkObject, bool correctLandmarkObject, float trueBearingToLandmark, float estimatedBearingToLandmark,
        float bearingErrorToLandmark, float absBearingErrorToLandmark, float selectedLandmarkDistance, float actualLandmarkDistance,
        float signedLandmarkDistanceError, float absoluteLandmarkDistanceError, float signedLandmarkOffsetDistance, float absoluteLandmarkOffsetDistance)
    {
        string baseFilePath = DATA_PATH;

        if (!Directory.Exists(baseFilePath))
        {
            Directory.CreateDirectory(baseFilePath);
        }

        string outputFilePath = baseFilePath + "/" + TRIAL_ASSESSMENT_FILE_NAME;
        string dataToExport = _baseParticipantData + "," + selectedPathLayout + "," + actualPathLayout
                + "," + correntPathLayout.ToString() + "," + segmentNumber.ToString() + "," + segmentTime.ToString()
                + "," + numberHints + "," + firstHintTimeSec.ToString() + "," + selectedTurnAngleToSegment.ToString(_dataDecimalPlaces)
                + "," + actualTurnAngleToSegment.ToString(_dataDecimalPlaces)
                + "," + selectedSegmentDistance.ToString(_dataDecimalPlaces) + "," + actualSegmentDistance.ToString(_dataDecimalPlaces)
                + "," + signedSegmentDistanceError.ToString(_dataDecimalPlaces) + "," + absoluteSegmentDistanceError.ToString(_dataDecimalPlaces)
                + "," + selectedLandmarkObject + "," + actualLandmarkObject
                + "," + correctLandmarkObject.ToString() + "," + trueBearingToLandmark.ToString(_dataDecimalPlaces)
                + "," + estimatedBearingToLandmark.ToString(_dataDecimalPlaces) + "," + bearingErrorToLandmark.ToString(_dataDecimalPlaces)
                + "," + absBearingErrorToLandmark.ToString(_dataDecimalPlaces) + "," + selectedLandmarkDistance.ToString(_dataDecimalPlaces)
                + "," + actualLandmarkDistance.ToString() + "," + signedLandmarkDistanceError.ToString(_dataDecimalPlaces)
                + "," + absoluteLandmarkDistanceError.ToString() + "," + signedLandmarkOffsetDistance.ToString(_dataDecimalPlaces)
                + "," + absoluteLandmarkOffsetDistance.ToString(_dataDecimalPlaces);

        string header = BASE_FILE_HEADER + "," + TRIAL_ASSESSMENT_HEADER;
        _CSVUtils.WriteDataToFile(dataToExport, outputFilePath, header);
    }

    #endregion
}
