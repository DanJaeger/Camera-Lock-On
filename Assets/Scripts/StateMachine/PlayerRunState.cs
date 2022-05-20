using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    const float _runSpeed = 6.0f;
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
   : base(currentContext, playerStateFactory) { }
    public override void CheckSwitchStates()
    {
        if (Context.Strafing)
        {
            SwitchState(Factory.Strafe());
        }
        else if (Context.Sprinting)
        {
            SwitchState(Factory.Sprint());
        }
        else if (!Context.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
    }

    public override void EnterState()
    {
        Debug.LogWarning("Estas en Run");
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
        Context.TargetSpeed = (Context.IsMovementPressed) ? _runSpeed : 0.0f;
        Context.HandleInput();
        Context.HandleRotation();
        CheckSwitchStates();
    }
}
