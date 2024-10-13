using System.Collections.Generic;
using UnityEngine;

public class ObjectRenderManager : MonoBehaviour
{
    public static ObjectRenderManager Instance { get; private set; }

    public Dictionary<PathSegmentData, ObjectRenderer> ActiveRenderTextures = new();
    [SerializeField] private GameObject _objectiveObjectParent;
    [SerializeField] private GameObject _segmentObjectParent;
    [SerializeField] private GameObject _renderSetupPrefab;

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

    void Start()
    {
        ActiveRenderTextures = new();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void CreateNewSegmentObject(PathSegmentData segmentData)
    {
        Vector3 position = new(0, 0, 0)
        {
            x = ActiveRenderTextures.Count * 0.5f
        };

        ObjectRenderer objectRenderer = Instantiate(_renderSetupPrefab, position, Quaternion.identity, 
            _segmentObjectParent.transform).GetComponent<ObjectRenderer>();
        objectRenderer.SpawnObject(segmentData.ObjectPrefab, segmentData.SegmentObjectRenderTexture);
        ActiveRenderTextures.Add(segmentData, objectRenderer);
    }

    public void ClearSegmentObjectRenderTextures()
    {
        foreach (ObjectRenderer objectRenderer in ActiveRenderTextures.Values)
        {
            Destroy(objectRenderer.gameObject);
        }
        ActiveRenderTextures.Clear();
    }
    public void CreateNewObjectiveObject(PathSegmentData segmentData)
    {
        Vector3 position = new(0, 0, 0)
        {
            x = ActiveRenderTextures.Count * 0.5f
        };

        ObjectRenderer objectRenderer = Instantiate(_renderSetupPrefab, position, Quaternion.identity,
            _objectiveObjectParent.transform).GetComponent<ObjectRenderer>();
        objectRenderer.SpawnObject(segmentData.ObjectiveObjectPrefab, segmentData.ObjectiveObjectRenderTexture);
        ActiveRenderTextures.Add(segmentData, objectRenderer);
    }

}
