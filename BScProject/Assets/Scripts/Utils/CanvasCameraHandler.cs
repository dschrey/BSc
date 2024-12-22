using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CanvasCameraHandler : MonoBehaviour
{

    [SerializeField] private Camera _canvasCamera;
    
    void OnEnable()
    {
        _canvasCamera = GetComponent<Camera>();
    }

    public Vector3 ScreenCoordinatesToWorldSpace(Vector2 screenPosition)
    {
        return _canvasCamera.ScreenToWorldPoint(screenPosition);
    }

    public Vector3 WorldCoordinatesToScreenSpace(Vector3 worldPosition)
    {
        return  _canvasCamera.WorldToScreenPoint(worldPosition);
    }

    public Camera GetCamera()
    {
        return _canvasCamera;
    }
}
