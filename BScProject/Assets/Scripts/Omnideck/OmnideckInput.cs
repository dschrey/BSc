using UnityEngine;
using Omnifinity.Omnideck;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;

public struct OmnideckInputState : IInputStateTypeInfo
{
    public readonly FourCC format => new('O', 'M', 'N', 'I');

    [InputControl(name = "movement", layout = "Vector2")]
    public Vector2 movement;
}

[InputControlLayout(stateType = typeof(OmnideckInputState))]
public class OmnideckInputDevice : InputDevice
{
    public Vector2Control Movement { get; private set; }
    protected override void FinishSetup()
    {
        base.FinishSetup();
        Movement = GetChildControl<Vector2Control>("movement");
    }
}

public class OmnideckInput : MonoBehaviour
{
    private OmnideckInputDevice m_omnideckInput;
    private OmnideckInterface m_omnideckInterface;

    void Start()
    {
        InputSystem.RegisterLayout<OmnideckInputDevice>("Omnideck");
        m_omnideckInput = InputSystem.AddDevice<OmnideckInputDevice>("Omnideck");
        if (!TryGetComponent<OmnideckInterface>(out m_omnideckInterface))
            Debug.LogError($"Could not find OmnideckInterface reference.");
    }

    void Update()
    {
        if (m_omnideckInput == null || m_omnideckInterface == null) return;

        Vector3 movementVector = m_omnideckInterface.GetCurrentOmnideckCharacterMovementVector();
        var state = new OmnideckInputState { movement = new (movementVector.x, movementVector.z) };
        InputSystem.QueueStateEvent(m_omnideckInput, state);
    }

    void OnDestroy()
    {
        if (m_omnideckInput != null)
        {
            InputSystem.RemoveDevice(m_omnideckInput);
        }
    }
}