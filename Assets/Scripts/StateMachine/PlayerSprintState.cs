using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprintState : PlayerBaseState
{
    const float _sprintSpeed = 8.0f;
    public PlayerSprintState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
   : base(currentContext, playerStateFactory) { }
    public override void CheckSwitchStates()
    {
        if (Context.Strafing)
        {
            SwitchState(Factory.Strafe());
        }
        else if (Context.IsMovementPressed && !Context.Sprinting)
        {
            SwitchState(Factory.Idle());
        }
        else if (!Context.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override void EnterState()
    {
        Debug.LogWarning("Estas en sprint");
        Context.CameraController.ToggleLockOn(false);
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        Context.TargetSpeed = (Context.IsMovementPressed) ? _sprintSpeed : 0.0f;
        Context.HandleInput();
        Context.HandleRotation();
        CheckSwitchStates();
    }
}
