using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CanvasCameraHandler : MonoBehaviour
{

    private Camera _canvasCamera;
    // Start is called before the first frame update
    
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
}
