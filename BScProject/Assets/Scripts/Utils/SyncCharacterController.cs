using UnityEngine;

public class CharacterControllerSync : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _XRCamera;

    private void Update()
    {
        if (_characterController == null || _XRCamera == null)
            return;

        Vector3 cameraPosition = _XRCamera.localPosition;
        _characterController.center = new Vector3(cameraPosition.x, _characterController.center.y, cameraPosition.z);
    }
}
