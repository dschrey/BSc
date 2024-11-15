using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }
    [SerializeField] private ExperimentSettings _experimentData;
    private string _experimentSettingsPath;
    private string _experimentResultsPath;
    private string _assessmentFilePath;


   // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
    
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

        _experimentSettingsPath = Application.persistentDataPath + "experimentSettings.json";
        _experimentResultsPath = Application.persistentDataPath + "/Assessments";
    }

    private void Start() 
    {
        if (! Directory.Exists(_experimentResultsPath))
        {
            Directory.CreateDirectory(_experimentResultsPath);
        }
        LoadExperimentSettings();
    }

   // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SaveExperimentSettings()
    {
        ExperimentSettingsData data = new()
        {
            CompletedAssessments = _experimentData.CompletedAssessments,
            PlayerDetectionRadius = _experimentData.PlayerDetectionRadius,
            ObjectiveRevealTime = _experimentData.ObjectiveRevealTime,
            MovementSpeedMultiplier = _experimentData.MovementSpeedMultiplier
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_experimentSettingsPath, json);
    }


    public void LoadExperimentSettings()
    {
        if (File.Exists(_experimentSettingsPath))
        {
            string json = File.ReadAllText(_experimentSettingsPath);

            ExperimentSettingsData data = JsonUtility.FromJson<ExperimentSettingsData>(json);

            _experimentData.PlayerDetectionRadius = data.PlayerDetectionRadius;
            _experimentData.ObjectiveRevealTime = data.ObjectiveRevealTime;
            _experimentData.MovementSpeedMultiplier = data.MovementSpeedMultiplier;
            _experimentData.CompletedAssessments = data.CompletedAssessments;
        } 
        else
        {
            SaveExperimentSettings();
        }
    }

    public void SaveAssessmentData(AssessmentData data)
    {
        string jsonData = JsonUtility.ToJson(data, true);
 
        string filePath = _experimentResultsPath + "/" + $"Assessment_{ExperimentManager.Instance.ExperimentSettings.CompletedAssessments}.json";
        File.WriteAllText(filePath, jsonData);
        _assessmentFilePath = filePath;

        Debug.Log($"Assessment data saved to: {filePath}");
    }

    public void ResetAssessmentFilePath()
    {
        _assessmentFilePath = null;
    }

}
