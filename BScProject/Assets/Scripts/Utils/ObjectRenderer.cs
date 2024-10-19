using System;
using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [SerializeField] private Camera _renderCamera;
    [SerializeField] private Transform _objectSpawnpoint;

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(GameObject objectPrefab, RenderTexture renderTexture)
    {
        // Instantiate the object at the spawn point
        GameObject spawnedObject = Instantiate(objectPrefab, _objectSpawnpoint);
        SetLayerRecursively(spawnedObject, LayerMask.NameToLayer("ObjectRendering"));

        // Calculate bounds of the spawned object
        Bounds objectBounds = CalculateObjectBounds(spawnedObject);

        // Adjust the camera position to fit the entire object in view
        AdjustCameraToFitObject(objectBounds);

        _renderCamera.targetTexture = renderTexture;
        _renderCamera.clearFlags = CameraClearFlags.SolidColor;
        _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        _renderCamera.cullingMask = LayerMask.GetMask("ObjectRendering");
    }

        // Helper method to calculate the bounds of the object
    private Bounds CalculateObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    // Adjust the camera position to ensure the object fits within the view
    private void AdjustCameraToFitObject(Bounds objectBounds)
    {
        float objectSize = objectBounds.extents.magnitude;
        float distanceToFitObject = objectSize / Mathf.Tan(Mathf.Deg2Rad * _renderCamera.fieldOfView / 2f);

        // Position the camera at an appropriate distance to fit the object
        _renderCamera.transform.position = objectBounds.center - _renderCamera.transform.forward * distanceToFitObject;

        // Optional: Adjust camera rotation to perfectly look at the object
        _renderCamera.transform.LookAt(objectBounds.center);
    }

    public RenderTexture GetRenderTexture()
    {
        return _renderCamera.targetTexture;
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
