
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRInteractionToggle : MonoBehaviour
{
    [SerializeField] private XRBaseInteractor m_LeftHandInteractor;
    [SerializeField] private UnityEngine.InputSystem.InputActionReference m_LeftHandActivateAction;
    [SerializeField] private XRBaseInteractor m_RightHandInteractor;
    [SerializeField] private UnityEngine.InputSystem.InputActionReference m_RightHandActivateAction;

    public InputDeviceCharacteristics m_leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand;
    public InputDeviceCharacteristics m_rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand;

    public InputDevice m_leftInputDevice;
    public InputDevice m_rightInputDevice;
    private bool _isRightActive = true;

    private XRBaseInteractor _currentlyEnabledInteractor = null;
    private  UnityEngine.InputSystem.InputActionReference _currentlyValidActivateAction = null;

    private void Start()
    {
        AssignControllers();

        m_RightHandInteractor.enabled = _isRightActive;
        m_LeftHandInteractor.enabled = !_isRightActive;
        _currentlyEnabledInteractor = m_RightHandInteractor;
        _currentlyValidActivateAction = m_RightHandActivateAction;
        SetActiveInteractor(true);
    }

    private void Update()
    {
        if (!m_leftInputDevice.isValid || !m_rightInputDevice.isValid)
        {
            AssignControllers();
        }

        if (IsAnyButtonPressed(m_rightInputDevice))
        {
            SetActiveInteractor(true);
        }
        else if (IsAnyButtonPressed(m_leftInputDevice))
        {
            SetActiveInteractor(false);
        }
    }

    private void AssignControllers()
    {
        m_rightInputDevice = GetController(m_rightControllerCharacteristics);
        m_leftInputDevice = GetController(m_leftControllerCharacteristics);
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

    private void SetActiveInteractor(bool activateRight)
    {
        if (_isRightActive == activateRight) return;

        _isRightActive = activateRight;
        m_RightHandInteractor.enabled = activateRight;
        m_LeftHandInteractor.enabled = !activateRight;
        if (_isRightActive)
        {
            _currentlyEnabledInteractor = m_RightHandInteractor;
            _currentlyValidActivateAction = m_RightHandActivateAction;
        }
        else
        {
            _currentlyEnabledInteractor = m_LeftHandInteractor;
            _currentlyValidActivateAction = m_LeftHandActivateAction;
        }
        UpdateUIInteractor();

    }
    
    public void UpdateUIInteractor()
    {
        UIObjectSelection objectSelectionUI = FindObjectOfType<UIObjectSelection>();
        if (objectSelectionUI != null)
        {
            if (_currentlyEnabledInteractor.TryGetComponent<NearFarInteractor>(out var nearFarInteractor))
            {
                objectSelectionUI.UIInteractor = nearFarInteractor;
                objectSelectionUI.PlaceAction = _currentlyValidActivateAction;
                Debug.Log($"Updating Interactor to {_currentlyEnabledInteractor.gameObject.name}");
            }
            else
            {
                Debug.LogError($"Invalid interactor component active.");
            }
        }

    }
}
