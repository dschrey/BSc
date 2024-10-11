using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _fixedXAngle = 0f;
    [SerializeField] private float _fixedZAngle = 0f;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += _rotationSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(_fixedXAngle, currentRotation.y, _fixedZAngle);
    }
}
