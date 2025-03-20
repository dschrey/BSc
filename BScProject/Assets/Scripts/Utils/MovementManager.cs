using Omnifinity.Omnideck;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class LocomotionManager : MonoBehaviour
{
    [SerializeField] private FloorType _activeType = FloorType.Default;

    [Header("Default"), SerializeField]
    private CharacterControllerSync _characterControllerSync;
    
    [Header("Omnideck"), SerializeField]
    private OmnideckInterface _Interface;

    [SerializeField]
    private ContinuousMoveProvider _moveprovider;


    public bool HandleFloorBasedMovement(FloorType floorType)
    {
        _activeType = floorType;
        if (floorType == FloorType.Default)
        {
            //if (_characterControllerSync == null)
            //{
            //    Debug.LogError("Could not find Character Controller Sync component.");
            //    return false;
            //}
            //_characterControllerSync.enabled = true;
        }
        else
        {

            if (_Interface == null)
            {
                Debug.LogError($"Omnideck interace not found.");
                return false;
            }
            _Interface.enabled = true;

            if (_moveprovider == null)
            {
                Debug.LogError("Could not find Omnideck Continuous Move provider.");
                return false;
            }

            _moveprovider.enabled = true;
        }

        return true;
    }
}
