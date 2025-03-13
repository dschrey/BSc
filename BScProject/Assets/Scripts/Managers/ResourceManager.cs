using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<RenderObject> HoverObjects = new();
    public List<RenderObject> LandmarkObjects = new();
    public List<PathData> LoadedPaths = new();
    public List<ParticipantData> ExperimentData = new();

    private List<Color> _colors= new ()
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        new Color(1f, 0.5f, 0f),  // Orange
        new Color(0.5f, 0f, 0.5f), // Purple
        new Color(0.8f, 0.4f, 0.2f), // Brown
        Color.white
    };

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
        LoadLandmarkObjects();
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

    private void LoadLandmarkObjects()
    {
        LandmarkObjects.AddRange(LoadRenderObjects("LandmarkObjects"));
        Debug.Log($"Loaded {LandmarkObjects.Count} landmark objects.");
    }

    private void LoadPaths()
    {
        PathData[] paths = Resources.LoadAll<PathData>("PathData");
        int count = 0;
        int color = 0;
        foreach (PathData path in paths)
        {
            path.PathID = count;
            if (color == _colors.Count) color = 0;
            path.PathColor = _colors[color];
            color++;
            count++;
        }
        LoadedPaths.AddRange(paths);
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

    public void FreeRenderTextures()
    {
        HoverObjects.ForEach(r =>
        {
            if (r.RenderTexture != null)
            {
                r.RenderTexture.Release();
                r.RenderTexture = null;
            }
        });

        LandmarkObjects.ForEach(r =>
        {
            if (r.RenderTexture != null)
            {
                r.RenderTexture.Release();
                r.RenderTexture = null;
            }
        });
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
        return LoadedPaths.Find(p => p.PathName == pathName);
    }


}
