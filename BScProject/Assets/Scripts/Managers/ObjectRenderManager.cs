using System.Collections.Generic;
using UnityEngine;

public class ObjectRenderManager : MonoBehaviour
{
    [SerializeField] private HashSet<GameObject> _activeObjectRenderings = new();
    [SerializeField] private GameObject _objectRendererPrefab;


    void OnDestroy()
    {
        foreach (GameObject renderObject in _activeObjectRenderings)
        {
            Destroy(renderObject);
        }
        _activeObjectRenderings.Clear();
    }

    public void InitializeRenderObjects(PathObjects pathObjects)
    {
        if ((pathObjects & PathObjects.Hover) != 0)
        {
            foreach (var renderObject in ResourceManager.Instance.HoverObjects)
            {
                renderObject.RenderTexture = CreateObjectToRender(renderObject.gameObject);
            }
        }

        if ((pathObjects & PathObjects.Landmarks) != 0)
        {
            foreach (var renderObject in ResourceManager.Instance.LandmarkObjects)
            {
                renderObject.RenderTexture = CreateObjectToRender(renderObject.gameObject);
            }
        }
    }

    public RenderTexture CreateObjectToRender(GameObject objectPrefab)
    {
        RenderTexture renderTexture = new(256, 256, 24);

        Vector3 position = new(0, 0, 0)
        {
            x = _activeObjectRenderings.Count
        };

        GameObject objectRender = Instantiate(_objectRendererPrefab, transform);
        objectRender.transform.localPosition = position;
        Utils.SetLayerRecursively(objectRender, LayerMask.NameToLayer("ObjectRendering"));
        _activeObjectRenderings.Add(objectRender);

        if (objectRender.TryGetComponent<ObjectRenderer>(out var objectRenderer))
        {
            objectRenderer.Initialize(objectPrefab, renderTexture);
        }
        else
        {
            Debug.LogError("CreateNewRenderTexture : ObjectRenderer component missing.");
        }

        return renderTexture;
    }
    
}
