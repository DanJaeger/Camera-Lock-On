using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
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
        }else if (Context.IsMovementPressed)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void EnterState()
    {
        Context.TargetSpeed = 0.0f;
        Debug.LogWarning("Estas en Idle");
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        Context.HandleInput();
        CheckSwitchStates();
    }
}
