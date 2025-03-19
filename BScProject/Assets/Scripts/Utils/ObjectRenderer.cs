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

    public RenderTexture GetRenderTexture()
    {
        return _renderCamera.targetTexture;
    }

}
