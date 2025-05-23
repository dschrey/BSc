using Omnifinity.Omnideck;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public enum LocomotionMethod
{
    Walking,
    Omnideck
}

public class LocomotionManager : MonoBehaviour
{
    [Header("Omnideck"), SerializeField]
    private OmnideckInterface _Interface;

    [SerializeField]
    private ContinuousMoveProvider _moveProvider;

    private LocomotionMethod _currentLocomotionMethod;

    public bool SetLocomotionMethod()
    {
        _currentLocomotionMethod = DataManager.Instance.StudyData.LocomotionMethod;

        switch (_currentLocomotionMethod)
        {
            case LocomotionMethod.Omnideck:
                if (_Interface == null)
                {
                    Debug.LogError($"Omnideck interface not found.");
                    return false;
                }
                _Interface.enabled = true;

                if (_moveProvider == null)
                {
                    Debug.LogError("Could not find omnideck move provider.");
                    return false;
                }
                _moveProvider.enabled = true;
                break;
        }
        return true;
    }
}
