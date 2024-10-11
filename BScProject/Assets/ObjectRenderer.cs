using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [SerializeField] private Camera _renderCamera;
    [SerializeField] private GameObject _objectSpawnpoint;

    public ObjectRenderer SpawnObject(GameObject objectPrefab, RenderTexture renderTexture)
    {
        Instantiate(objectPrefab, _objectSpawnpoint.transform);
        // ScaleToCameraView(spawnedObject);

        _renderCamera.targetTexture = renderTexture;
        return this;
    }

    public RenderTexture GetRenderTexture()
    {
        return _renderCamera.targetTexture;
    }
}
