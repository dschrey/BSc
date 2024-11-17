using UnityEngine;

public class CharacterControllerSync : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _XRCameraOffset;


    private void Update()
    {
        if (_characterController == null || _XRCameraOffset == null)
            return;

        Vector3 cameraPosition = _XRCameraOffset.localPosition;
        _characterController.center = new Vector3(cameraPosition.x, _characterController.center.y, cameraPosition.z);
    }
}
