using System.Collections.Generic;
using UnityEngine;

public class ObjectRenderManager : MonoBehaviour
{
    public List<GameObject> ActiveObjectRenderings;
    [SerializeField] private Transform _objectiveObjectParent;
    [SerializeField] private Transform _segmentObjectParent;
    [SerializeField] private GameObject _objectRendererPrefab;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        ActiveObjectRenderings = new();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public RenderTexture CreateNewRenderTexture(PathSegmentData segmentData, bool isObjectiveObject = false)
    {
        RenderTexture renderTexture = new(256, 256, 24);
        GameObject objectRender;
        if (isObjectiveObject)
        {
            objectRender = Instantiate(_objectRendererPrefab, _objectiveObjectParent);
            objectRender.GetComponent<ObjectRenderer>().Initialize(segmentData.ObjectiveObjectPrefab, renderTexture);
        }
        else
        {
            objectRender = Instantiate(_objectRendererPrefab, _segmentObjectParent);
            objectRender.GetComponent<ObjectRenderer>().Initialize(segmentData.ObjectPrefab, renderTexture);
        }
        ActiveObjectRenderings.Add(objectRender);

        return renderTexture;
    }
}
