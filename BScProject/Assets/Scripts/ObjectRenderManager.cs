using System.Collections.Generic;
using UnityEngine;

public class ObjectRenderManager : MonoBehaviour
{
    public List<GameObject> ActiveObjectRenderings;
    [SerializeField] private GameObject _objectRendererPrefab;

    // ---------- Unity Methods ------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        ActiveObjectRenderings = new List<GameObject>();
    }

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public RenderTexture CreateNewRenderTexture(PathSegmentData segmentData, bool isObjectiveObject = false)
    {
        RenderTexture renderTexture = new(256, 256, 24);
        
        // Transform objectParent = isObjectiveObject ? _objectiveObjectParent : _segmentObjectParent;
        Vector3 position = new(0, 0, 0)
        {
            x = ActiveObjectRenderings.Count
        };

        GameObject objectRender = Instantiate(_objectRendererPrefab, transform);
        objectRender.transform.localPosition = position;
        SetLayerRecursively(objectRender, LayerMask.NameToLayer("ObjectRendering"));
        ActiveObjectRenderings.Add(objectRender);
    
        if (objectRender.TryGetComponent<ObjectRenderer>(out var objectRenderer))
        {
            GameObject objectPrefab = isObjectiveObject ? segmentData.ObjectiveObjectPrefab : segmentData.ObjectPrefab;
            objectRenderer.Initialize(objectPrefab, renderTexture);
        }
        else
        {
            Debug.LogError("CreateNewRenderTexture : ObjectRenderer component missing.");
        }

        return renderTexture;
    }

    public void ClearRenderTextures()
    {
        foreach (GameObject renderObject in ActiveObjectRenderings)
        {
            Destroy(renderObject);
        }
        ActiveObjectRenderings.Clear();
    }

    private void SetLayerRecursively(GameObject obj, int layerIndex)
    {
        obj.layer = layerIndex;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layerIndex);
        }
    }   

}
