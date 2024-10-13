using System.IO;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }
    [SerializeField] private ExperimentSettings _experimentData;
    private string _experimentSettingsPath;
    private string _experimentResultsPath;
    public string AssessmentFile;


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
        _experimentResultsPath = Application.persistentDataPath + "Results";
    }

    private void Start() 
    {
        AssessmentFile = null;
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
        if (! Directory.Exists(_experimentResultsPath))
        {
            Directory.CreateDirectory(_experimentResultsPath);
        }

        string jsonData = JsonUtility.ToJson(data, true);
        string filePath = _experimentResultsPath + "/" + $"Assessment_{data.DateTime}.json";
        AssessmentFile = filePath;
        if (!AssessmentFile.IsNullOrEmpty())
        {
            File.WriteAllText(AssessmentFile, jsonData);
        }
        else
        {
            File.WriteAllText(filePath, jsonData);
            AssessmentFile = filePath;
        }

        Debug.Log($"Assessment data saved to: {filePath}");
    }

}
