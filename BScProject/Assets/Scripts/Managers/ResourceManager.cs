using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<RenderObject> HoverObjects = new();
    public List<RenderObject> LandmarkObjects = new();
    public List<PathData> LoadedPaths = new();
    public List<ParticipantData> ExperimentData = new();

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Awake()
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
    }

    private void OnDestroy()
    {
        HoverObjects.ForEach(x =>
        {
            if (x.RenderTexture != null)
            {
                x.RenderTexture.Release();
            }
        });

        LandmarkObjects.ForEach(x =>
        {
            if (x.RenderTexture != null)
            {
                x.RenderTexture.Release();
            }
        });
    }


    private void Start()
    {
        LoadParticipantSequences();
        LoadObjectiveObjects();
        LoadSegmentObjects();
        LoadPaths();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void LoadParticipantSequences()
    {
        ExperimentData.AddRange(LoadParticipantData("ParticipantData"));
        Debug.Log($"Loaded {ExperimentData.Count} sequences.");
    }

    private void LoadObjectiveObjects()
    {
        HoverObjects.AddRange(LoadRenderObjects("ObjectiveObjects"));
        Debug.Log($"Loaded {HoverObjects.Count} hover objects.");
    }

    private void LoadSegmentObjects()
    {
        LandmarkObjects.AddRange(LoadRenderObjects("SegmentObjects"));
        Debug.Log($"Loaded {LandmarkObjects.Count} landmark objects.");
    }

    private void LoadPaths()
    {
        LoadedPaths.AddRange(Resources.LoadAll<PathData>("PathData"));
        Debug.Log($"Loaded {LoadedPaths.Count} path data.");
    }

    private List<RenderObject> LoadRenderObjects(string jsonFileName)
    {
        GameObject[] objectiveObj = Resources.LoadAll<GameObject>(jsonFileName);

        List<RenderObject> renderObjects = new();
        int count = 0;
        foreach (GameObject obj in objectiveObj)
        {
            RenderObject objectiveObject = obj.GetComponent<RenderObject>();
            objectiveObject.ID = count;

            // TODO initialize this only in ExperimentScene
            // ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();
            // objectiveObject.RenderTexture = objectRenderManager.CreateNewRenderTexture(obj);

            renderObjects.Add(objectiveObject);
            count++;
        }

        return renderObjects;
    }

    public void InitializeRenderObjects()
    {
        ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();
        if (objectRenderManager == null)
        {
            Debug.LogError("Could not find ObjectRenderManager.");
            return;
        }

        LandmarkObjects.ForEach(r => r.RenderTexture = objectRenderManager.CreateNewObjectRender(r.gameObject));
        HoverObjects.ForEach(r => r.RenderTexture = objectRenderManager.CreateNewObjectRender(r.gameObject));
    }

    public RenderTexture GetHoverObjectRenderTexture(int id)
    {
        foreach (var obj in HoverObjects)
        {
            if (obj.ID == id)
                return obj.RenderTexture;
        }

        return null;
    }

    public GameObject GetHoverObject(int id)
    {
        return HoverObjects.Find(obj => obj.ID == id).gameObject;
    }

    public List<RenderObject> ShuffleHoverObjects(int seed = -1)
    {
        return Utils.Shuffle(HoverObjects, seed);
    }

    public RenderTexture GetLandmarkObjectRenderTexture(int id)
    {
        foreach (var obj in LandmarkObjects)
        {
            if (obj.ID == id)
                return obj.RenderTexture;
        }

        return null;
    }

    public GameObject GetLandmarkObject(int id)
    {
        return LandmarkObjects.Find(obj => obj.ID == id).gameObject;
    }

    public List<RenderObject> ShuffleLandmarkObjects(int seed = -1)
    {
        return Utils.Shuffle(LandmarkObjects, seed);
    }


    /// <summary>
    /// Loads the participant data from a JSON file in the Resources folder.
    /// </summary>
    /// <param name="jsonFileName">Name of the JSON file without extension, e.g. "Participants"</param>
    /// <returns>List of ExperimentData with the settings loaded from JSON.</returns>
    private List<ParticipantData> LoadParticipantData(string jsonFileName)
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(jsonFileName);
        if (jsonTextAsset == null)
        {
            Debug.LogError("Could not find " + jsonFileName + " in the Resources folder.");
            return null;
        }

        ParticipantDataWrapper wrapper = JsonUtility.FromJson<ParticipantDataWrapper>(jsonTextAsset.text);
        if (wrapper == null || wrapper.participants == null)
        {
            Debug.LogError("Failed to parse participants from JSON.");
            return null;
        }

        foreach (ParticipantData experimentData in wrapper.participants)
        {
            if (experimentData.paths != null)
            {
                foreach (Trail trail in experimentData.paths)
                {
                    string floor = trail.floor == 0 ? "Reg" : "Omni";
                    trail.selectionName = $"{experimentData.id}_{trail.name}_{floor}";
                }
            }
            else
            {
                Debug.LogWarning("No trials found in the experiment data.");
            }
        }
        return wrapper.participants;
    }

    public ParticipantData GetExperimentData(int ID)
    {
        return ExperimentData.Find(d => d.id == ID); ;
    }

    public PathData LoadPathData(string pathName)
    {
        return LoadedPaths.Find(p => p.Name == pathName);
    }

    public Trail LoadTrail(int dataID, string pathName)
    {
        ParticipantData data = ExperimentData.Find(d => d.id == dataID);
        foreach (Trail trail in data.paths)
        {
            if (trail.name == pathName)
                return trail;
        }
        return null;
    }
    
}
