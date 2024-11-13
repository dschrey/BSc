using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private CanvasCameraHandler _canvasCamera;

    void OnEnable()
    {
        _canvasCamera = FindObjectOfType<CanvasCameraHandler>();
        if (_canvasCamera == null)
        {
            Debug.LogError($"Canvas camera could not be found.");
        }
    }

    public Vector3 DragObjectIn3DSpace(Vector2 screenPosition)
    {
        Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(screenPosition);
        worldPosition.y = transform.localScale.y / 2;
        transform.position = worldPosition;
        return transform.position;
    }
    


}
