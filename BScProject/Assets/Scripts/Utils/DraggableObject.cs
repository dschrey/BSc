using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private CanvasCameraHandler _canvasCamera;


    public Vector3 DragObjectIn3DSpace(Vector2 screenPosition)
    {
        Vector3 worldPosition = _canvasCamera.ScreenCoordinatesToWorldSpace(screenPosition);
        worldPosition.y = transform.localScale.y / 2;
        transform.position = worldPosition;
        return transform.position;
    }
    


}
