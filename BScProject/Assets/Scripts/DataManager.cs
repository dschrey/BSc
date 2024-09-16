using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public static DataManager Instance { get; private set; }
//     // [SerializeField] private ExperimentSettings _experimentData;
//     // [SerializeField] private PlayerSettings _playerSettings;
//     private string _experimentFilePath;
//     private string _playerSettingsFilePath;

//     // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------
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

//         _experimentFilePath = Application.persistentDataPath + "/ExperimentSettings.json";
//         _playerSettingsFilePath = Application.persistentDataPath + "/PlayerSettings.json";
    }

//     private void Start() 
//     {
//         LoadExperimentSettings();
//         LoadPlayerSettings();
//     }

//     // ---------- Listener Methods ------------------------------------------------------------------------------------------------------------------------



//     public void SaveExperimentSettings()
//     {
//         ExperimentSettingsData data = new()
//         {
//             UseFixedSpawningSphere = _experimentData.UseGlobalSpawningSphere,
//             SpawnRadius = _experimentData.SpawnRadius,
//             MinSpawnDistance = _experimentData.MinSpawnDistance,
//             UseLifeSpanTimer = _experimentData.UsePickupTimer,
//             PickupDuration = _experimentData.PickupTime,
//             UseVisibilityTimer = _experimentData.UseVisibilityTimer,
//             ItemVisibilityDuration = _experimentData.ItemVisibilityDuration,
//             UseItemProximity = _experimentData.UseItemProximityDisplay,
//             ItemProximityDistance = _experimentData.ItemProximityDistance
//         };

//         string json = JsonUtility.ToJson(data, true);
//         File.WriteAllText(_experimentFilePath, json);

//     }

//     public void LoadExperimentSettings()
//     {
//         if (File.Exists(_experimentFilePath))
//         {
//             string json = File.ReadAllText(_experimentFilePath);

//             ExperimentSettingsData data = JsonUtility.FromJson<ExperimentSettingsData>(json);

//             _experimentData.UseGlobalSpawningSphere = data.UseFixedSpawningSphere;
//             _experimentData.SpawnRadius = data.SpawnRadius;
//             _experimentData.MinSpawnDistance = data.MinSpawnDistance;
//             _experimentData.UsePickupTimer = data.UseLifeSpanTimer;
//             _experimentData.PickupTime = data.PickupDuration;
//             _experimentData.UseVisibilityTimer = data.UseVisibilityTimer;
//             _experimentData.ItemVisibilityDuration = data.ItemVisibilityDuration;
//             _experimentData.UseItemProximityDisplay = data.UseItemProximity;
//             _experimentData.ItemProximityDistance = data.ItemProximityDistance;
//         } 
//         else
//         {
//             SaveExperimentSettings();
//         }
//     }
//     public void SavePlayerSettings()
//     {
//         PlayerSettingsData data = new()
//         {
//             MasterVolume = _playerSettings.MasterVolume,
//             EffectsVolume = _playerSettings.EffectsVolume,
//             AmbientVolume = _playerSettings.AmbientVolume,
//             UIVolume = _playerSettings.AmbientVolume
//         };

//         string json = JsonUtility.ToJson(data, true);
//         File.WriteAllText(_playerSettingsFilePath, json);

//     }

//     public void LoadPlayerSettings()
//     {
//         if (File.Exists(_playerSettingsFilePath))
//         {
//             string json = File.ReadAllText(_playerSettingsFilePath);

//             PlayerSettingsData data = JsonUtility.FromJson<PlayerSettingsData>(json);

//             _playerSettings.MasterVolume = data.MasterVolume;
//             _playerSettings.EffectsVolume = data.EffectsVolume;
//             _playerSettings.AmbientVolume = data.AmbientVolume;
//             _playerSettings.UIVolume = data.UIVolume;
//         } 
//         else
//         {
//             SavePlayerSettings();
//         }
//     }
}
