
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public enum PrimaryHand
{
    Right,
    Left
}

public class XRControllerManager : MonoBehaviour
{
    [SerializeField] private NearFarInteractor _leftHandInteractor;
    [SerializeField] private UnityEngine.InputSystem.InputActionReference _leftHandActivateAction;
    [SerializeField] private NearFarInteractor _rightHandInteractor;
    [SerializeField] private UnityEngine.InputSystem.InputActionReference _rightHandActivateAction;
    public UnityEngine.InputSystem.InputActionReference DebugActivateAction;
    private PrimaryHand _primaryHand;
    public InputDeviceCharacteristics _leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand;
    public InputDeviceCharacteristics _rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand;
    public InputDevice _leftInputDevice;
    public InputDevice _rightInputDevice;
    // private bool _isRightActive = true;

    // private XRBaseInteractor _currentlyEnabledInteractor = null;
    // private UnityEngine.InputSystem.InputActionReference _currentlyValidActivateAction = null;
    public NearFarInteractor ActiveXRInteractor => _primaryHand == PrimaryHand.Right ? _rightHandInteractor : _leftHandInteractor;
    public UnityEngine.InputSystem.InputActionReference ActiveActivateAction => 
        _primaryHand == PrimaryHand.Right ? _rightHandActivateAction : _leftHandActivateAction;
    private void Start()
    {
        AssignControllers();
        SetPrimaryHand(PrimaryHand.Right);
        // SetActiveInteractor(true);
    }

    public void SetPrimaryHand(PrimaryHand hand)
    {
        _primaryHand = hand;
        if (_primaryHand == PrimaryHand.Right)
        {
            _rightHandInteractor.enabled = true;
            _leftHandInteractor.enabled = false;
        }
        else
        {
            _rightHandInteractor.enabled = false;
            _leftHandInteractor.enabled = true;
        }
    }

    private void AssignControllers()
    {
        _rightInputDevice = GetController(_rightControllerCharacteristics);
        _leftInputDevice = GetController(_leftControllerCharacteristics);
    }

    private InputDevice GetController(InputDeviceCharacteristics characteristics)
    {
        List<InputDevice> devices = new();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        return devices.Count > 0 ? devices[0] : new InputDevice();
    }

    private bool IsAnyButtonPressed(InputDevice controller)
    {
        if (!controller.isValid) return false;

        foreach (InputFeatureUsage<bool> button in new InputFeatureUsage<bool>[]
        {
            CommonUsages.primaryButton, CommonUsages.secondaryButton, CommonUsages.primaryTouch,
            CommonUsages.secondaryTouch, CommonUsages.gripButton, CommonUsages.triggerButton,
            CommonUsages.menuButton
        })
        {
            if (controller.TryGetFeatureValue(button, out bool pressed) && pressed)
            {
                return true;
            }
        }
        return false;
    }

    // private void SetActiveInteractor(bool activateRight)
    // {
    //     if (_isRightActive == activateRight) return;

    //     _isRightActive = activateRight;
    //     m_RightHandInteractor.enabled = activateRight;
    //     m_LeftHandInteractor.enabled = !activateRight;
    //     if (_isRightActive)
    //     {
    //         _currentlyEnabledInteractor = m_RightHandInteractor;
    //         _currentlyValidActivateAction = m_RightHandActivateAction;
    //     }
    //     else
    //     {
    //         _currentlyEnabledInteractor = m_LeftHandInteractor;
    //         _currentlyValidActivateAction = m_LeftHandActivateAction;
    //     }
    //     UpdateUIInteractor();

    // }

    // public void UpdateUIInteractor()
    // {
    //     UILandmarkSelection objectSelectionUI = FindObjectOfType<UILandmarkSelection>();
    //     if (objectSelectionUI != null)
    //     {
    //         if (_currentlyEnabledInteractor.TryGetComponent<NearFarInteractor>(out var nearFarInteractor))
    //         {
    //             objectSelectionUI.UIInteractor = nearFarInteractor;
    //             objectSelectionUI.PlaceAction = _currentlyValidActivateAction;
    //             Debug.Log($"Updating Interactor to {_currentlyEnabledInteractor.gameObject.name}");
    //         }
    //         else
    //         {
    //             Debug.LogError($"Invalid interactor component active.");
    //         }
    //     }

    // }
}
