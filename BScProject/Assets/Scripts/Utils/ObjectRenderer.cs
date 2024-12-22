using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [SerializeField] private Camera _renderCamera;
    [SerializeField] private Transform _objectSpawnpoint;

    // ---------- Class Methods ------------------------------------------------------------------------------------------------------------------------

    public void Initialize(GameObject objectPrefab, RenderTexture renderTexture)
    {
        GameObject spawnedObject = Instantiate(objectPrefab, _objectSpawnpoint);
        spawnedObject.AddComponent<Rotate>();
        Utils.SetLayerRecursively(spawnedObject, LayerMask.NameToLayer("ObjectRendering"));

        Bounds objectBounds = Utils.CalculateObjectBounds(spawnedObject);

        Utils.AdjustCameraToBounds(_renderCamera, objectBounds);

        _renderCamera.targetTexture = renderTexture;
        _renderCamera.clearFlags = CameraClearFlags.SolidColor;
        _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        _renderCamera.cullingMask = LayerMask.GetMask("ObjectRendering");
    }

    // private Bounds CalculateObjectBounds(GameObject obj)
    // {
    //     Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
    //     Bounds bounds = renderers[0].bounds;

    //     foreach (Renderer renderer in renderers)
    //     {
    //         bounds.Encapsulate(renderer.bounds);
    //     }

    //     return bounds;
    // }

    // private void AdjustCameraToFitObject(Bounds objectBounds)
    // {
    //     _renderCamera.orthographicSize = Mathf.Max(objectBounds.size.y, objectBounds.size.x) / 2f + 0.05f;

    //     _renderCamera.transform.position = new Vector3(objectBounds.center.x, objectBounds.center.y, _renderCamera.transform.position.z);
    //     _renderCamera.transform.LookAt(objectBounds.center);
    // }

    public RenderTexture GetRenderTexture()
    {
        return _renderCamera.targetTexture;
    }

}
