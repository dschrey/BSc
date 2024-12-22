using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<RenderObject> HoverObjects = new();
    public List<RenderObject> LandmarkObjects = new();
    public List<PathData> LoadedPaths = new();

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
        HoverObjects.ForEach(x => x.RenderTexture.Release());
        HoverObjects.Clear();
    }


    private void Start() 
    {
        LoadObjectiveObjects();
        LoadSegmentObjects();
        LoadPaths();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

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

    private List<RenderObject> LoadRenderObjects(string resourcePath)
    {
        GameObject[] objectiveObj = Resources.LoadAll<GameObject>(resourcePath);

        List<RenderObject> renderObjects = new();
        int count = 0;
        foreach (GameObject obj in objectiveObj)
        {
            RenderObject objectiveObject = obj.GetComponent<RenderObject>();
            objectiveObject.ID = count;

            ObjectRenderManager objectRenderManager = FindObjectOfType<ObjectRenderManager>();

            // TODO initialize this only in ExperimentScene
            objectiveObject.RenderTexture = objectRenderManager.CreateNewRenderTexture(obj);

            renderObjects.Add(objectiveObject);
            count++;
        }

        return renderObjects;
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
}
