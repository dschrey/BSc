using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }
    public ExperimentSettings ExperimentData;
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
        // LoadExperimentSettings();
    }

   // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SaveExperimentSettings()
    {
        Debug.Log($"Saving experiment Settings");
        ExperimentSettingsData data = new()
        {
            CompletedAssessments = ExperimentData.CompletedAssessments,
            PlayerDetectionRadius = ExperimentData.PlayerDetectionRadius,
            ObjectiveRevealTime = ExperimentData.ObjectiveRevealTime,
            MovementSpeedMultiplier = ExperimentData.MovementSpeedMultiplier
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

            ExperimentData.PlayerDetectionRadius = data.PlayerDetectionRadius;
            ExperimentData.ObjectiveRevealTime = data.ObjectiveRevealTime;
            ExperimentData.MovementSpeedMultiplier = data.MovementSpeedMultiplier;
            ExperimentData.CompletedAssessments = data.CompletedAssessments;
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
