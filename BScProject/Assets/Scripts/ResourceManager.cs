using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public List<RenderObject> ObjectiveObjects = new();
    public List<RenderObject> SegmentObjects = new();

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() 
    {
        ObjectiveObjects.ForEach(x => x.RenderTexture.Release());
        ObjectiveObjects.Clear();
    }


    private void Start() 
    {
        LoadObjectiveObjects();
        LoadSegmentObjects();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    private void LoadObjectiveObjects()
    {
        ObjectiveObjects.AddRange(LoadRenderObjects("ObjectiveObjects"));
    }

    private void LoadSegmentObjects()
    {
        SegmentObjects.AddRange(LoadRenderObjects("SegmentObjects"));
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
            objectiveObject.RenderTexture = objectRenderManager.CreateNewRenderTexture(obj);

            renderObjects.Add(objectiveObject);
        }

        return renderObjects;
    }

    public RenderTexture GetObjectiveRenderTexture(int id)
    {
        foreach (var obj in ObjectiveObjects)
        {
            if (obj.ID == id)
                return obj.RenderTexture;
        }

        return null;
    }

    public GameObject GetObjectiveObject(int id)
    {
        return ObjectiveObjects[id].gameObject;
    }

    public List<RenderObject> ShuffleObjectiveObjects(int seed = -1)
    {
        return Utils.Shuffle(ObjectiveObjects, seed);
    }

    public RenderTexture GetSegmentRenderTexture(int id)
    {
        foreach (var obj in SegmentObjects)
        {
            if (obj.ID == id)
                return obj.RenderTexture;
        }

        return null;
    }

    public GameObject GetSegmentObject(int id)
    {
        return SegmentObjects[id].gameObject;
    }

    public List<RenderObject> ShuffleSegmentObjects(int seed = -1)
    {
        return Utils.Shuffle(SegmentObjects, seed);
    }
}