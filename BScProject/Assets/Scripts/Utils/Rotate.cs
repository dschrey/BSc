using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _fixedXAngle = 0f;
    [SerializeField] private float _fixedZAngle = 0f;

    void Update()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += _rotationSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(_fixedXAngle, currentRotation.y, _fixedZAngle);
    }
}
