using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }
    public Settings Settings;
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

        _experimentSettingsPath = Application.persistentDataPath + "/Settings.json";
        _experimentResultsPath = Application.persistentDataPath + "/Assessments";
    }

    private void Start() 
    {
        if (! Directory.Exists(_experimentResultsPath))
        {
            Directory.CreateDirectory(_experimentResultsPath);
        }
        LoadSettings();
    }

   // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void SaveSettings()
    {
        Debug.Log($"Saving experiment Settings");
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
        _assessmentFilePath = filePath;

        Debug.Log($"Assessment data saved to: {filePath}");
    }

}
