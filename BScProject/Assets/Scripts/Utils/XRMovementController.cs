using UnityEngine;
using UnityEngine.XR;

public class XRMovementController : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

        InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        if (hmd.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 hmdPosition))
        {
            // Vector3 scaledMovement = hmdPosition * ExperimentManager.Instance.ExperimentSettings.MovementSpeedMultiplier;
            // scaledMovement.y = transform.position.y;
            // transform.position += scaledMovement;
            Vector3 currentPos = transform.position;
            currentPos.x = transform.position.x * DataManager.Instance.Settings.MovementSpeedMultiplier;
            currentPos.z = transform.position.z * DataManager.Instance.Settings.MovementSpeedMultiplier;
            transform.position = currentPos;
        }


    }
}
