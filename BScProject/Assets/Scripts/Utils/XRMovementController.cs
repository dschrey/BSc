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
            Vector3 scaledMovement = hmdPosition * ExperimentManager.Instance.ExperimentSettings.MovementSpeedMultiplier;
            transform.position += scaledMovement;
        }
    }
}