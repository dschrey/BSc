using UnityEngine;

public class CharacterControllerSync : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _XRCamera;
    [SerializeField] private GameObject _characterVisual;

    private void Update()
    {
        if (_characterController == null || _XRCamera == null)
            return;

        Vector3 cameraPosition = _XRCamera.localPosition;
        Vector3 position = new(cameraPosition.x, _characterController.center.y, cameraPosition.z);
        _characterController.center = position;
        position.y = 0.2f;
        _characterVisual.transform.position = position;
    }
}
