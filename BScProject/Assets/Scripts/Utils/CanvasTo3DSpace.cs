using UnityEngine;

public class CanvasTo3DSpace : MonoBehaviour
{
    public Vector3 CanvasObjectPosition;
    [SerializeField] private Camera orthoCamera;

    public void ScreenPositionToWorldSpace(Vector2 screenPosition)
    {
        Vector3 objectWorldPosition = transform.position;

        // Convert the world position to screen space
        Vector3 screenP = orthoCamera.WorldToScreenPoint(objectWorldPosition);
        Debug.Log($"Screen Space {screenP} - {screenPosition}");

        Vector3 worldPosition = orthoCamera.ScreenToWorldPoint(screenPosition);
        Debug.Log($"World Space {worldPosition}");

        worldPosition.y = transform.localScale.y / 2;
        transform.position = worldPosition;

    }


}
