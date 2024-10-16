using System;
using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    [SerializeField] private Camera _renderCamera;
    [SerializeField] private Transform _objectSpawnpoint;

    public void Initialize(GameObject objectPrefab, RenderTexture renderTexture)
    {
        Instantiate(objectPrefab, _objectSpawnpoint);
        _renderCamera.targetTexture = renderTexture;
    }

    public RenderTexture GetRenderTexture()
    {
        return _renderCamera.targetTexture;
    }

}
