using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using Omnifinity.Omnideck;

public class OmnideckContinuousMove : ContinuousMoveProvider
{
    [SerializeField]
    private OmnideckInterface _omnideckInterface;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        if (_omnideckInterface == null)
            return base.ComputeDesiredMove(new Vector2(0, 0));

        Vector3 omnideck_vector = _omnideckInterface.GetCurrentOmnideckCharacterMovementVector();
        Vector3 desired_move = base.ComputeDesiredMove(input);
         Debug.Log($"Omnideck Movement: ({input} : {omnideck_vector}) - Desired Move: {desired_move}");
        return desired_move;
    }
}