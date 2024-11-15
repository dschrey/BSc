using UnityEngine;

public class CharacterControllerSync : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform xrCamera;


    private void Update()
    {
        if (characterController == null || xrCamera == null)
            return;

        // Match the Character Controller's position to the XR Camera's X and Z, keeping its original Y
        Vector3 cameraPosition = xrCamera.position;
        characterController.center = new Vector3(cameraPosition.x, characterController.center.y, cameraPosition.z);
    }
}
