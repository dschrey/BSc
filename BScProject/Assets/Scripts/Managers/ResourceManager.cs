using System.Collections.Generic;
using UnityEngine;


public class ResourceManager : MonoBehaviour
{
    #region Variables
    public static ResourceManager Instance { get; private set; }
    public List<RenderObject> HoverObjects = new();
    public List<RenderObject> LandmarkObjects = new();
    public List<PathData> Paths = new();
    public List<PathSet> PathSets = new();
    public List<ParticipantData> ParticipantsData = new();

    private List<Color> _colors = new()
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

    #endregion
    #region Unity Methods

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


    private void Start()
    {
        LoadParticipantSequences();
        LoadObjectiveObjects();
        LoadLandmarkObjects();
        LoadPaths();
        LoadSets();
    }

    #endregion
    #region  Class Methods

    private void LoadParticipantSequences()
    {
        ParticipantsData.AddRange(LoadParticipantSchedule("ParticipantSchedule"));
        Debug.Log($"Loaded {ParticipantsData.Count} participants.");
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
        Paths.AddRange(paths);
        Debug.Log($"Loaded {Paths.Count} path data.");
    }

    private void LoadSets()
    {
        PathSets.AddRange(Resources.LoadAll<PathSet>("PathData"));
        Debug.Log($"Loaded {PathSets.Count} path sets.");
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
    private List<ParticipantData> LoadParticipantSchedule(string jsonFileName)
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
        return wrapper.participants;
    }

    public ParticipantData GetParticipantData(int participantNumber)
    {
        return ParticipantsData.Find(d => d.id == participantNumber); ;
    }

    public PathData GetPathData(string pathName)
    {
        return Paths.Find(p => p.PathName == pathName);
    }

    public PathSet GetPathSet(string setName)
    {
        return PathSets.Find(set => set.SetName == setName);
    }

    #endregion
}
