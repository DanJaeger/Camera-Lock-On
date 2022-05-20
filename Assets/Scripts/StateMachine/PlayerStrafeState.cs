using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStrafeState : PlayerBaseState
{
    const float _strafeSpeed = 2.0f;
    public PlayerStrafeState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
   : base(currentContext, playerStateFactory) { }
    public override void CheckSwitchStates()
    {
        if (!Context.IsMovementPressed && !Context.Strafing)
        {
            SwitchState(Factory.Idle());
        }
        else if (Context.Sprinting)
        {
            SwitchState(Factory.Sprint());
        }
        else if (Context.IsMovementPressed && !Context.Strafing)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void EnterState()
    { 
        Debug.LogWarning("Estas en strafe");
        Context.CameraController.ToggleLockOn(!Context.CameraController.LockedOn);
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        Context.TargetSpeed = (Context.IsMovementPressed) ? _strafeSpeed : 0.0f;
        HandleInput();
        HandleRotation();
        CheckSwitchStates();
    }
    void HandleInput()
    {
        Context.Sprinting = Context.InputManager.Sprint && Context.IsMovementPressed;
        Context.Strafing = Context.InputManager.LockOn && !Context.Sprinting;
    }
    void HandleRotation()
    {
        if (Context.CameraController.Target != null)
        {
            Vector3 toTarget = Context.CameraController.Target.TargetTransform.position - Context.transform.position;
            Vector3 planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            Context.TargetRotation = Quaternion.LookRotation(planarToTarget, Vector3.up);
            Context.NewRotation = Quaternion.Slerp(Context.transform.rotation, Context.TargetRotation, Time.deltaTime * Context.RotationSharpness);
            Context.transform.rotation = Context.NewRotation;
        }
    }
}
