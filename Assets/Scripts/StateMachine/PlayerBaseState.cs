using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    private PlayerStateMachine _context;
    private PlayerStateFactory _factory;

    public PlayerStateMachine Context { get => _context; set => _context = value; }
    public PlayerStateFactory Factory { get => _factory; set => _factory = value; }

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _context = currentContext;
        _factory = playerStateFactory;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();
        Context.CurrentState = newState;
    }
}
