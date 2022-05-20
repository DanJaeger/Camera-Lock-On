using System.Collections;
using System.Collections.Generic;
using UnityEngine;
enum PlayerStates
{
    idle,
    run,
    sprint,
    strafe
}
public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();
    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[PlayerStates.idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.run] = new PlayerRunState(_context, this);
        _states[PlayerStates.sprint] = new PlayerSprintState(_context, this);
        _states[PlayerStates.strafe] = new PlayerStrafeState(_context, this);
    }
    public PlayerBaseState Idle()
    {
        return _states[PlayerStates.idle];
    }
    public PlayerBaseState Run()
    {
        return _states[PlayerStates.run];
    }
    public PlayerBaseState Sprint()
    {
        return _states[PlayerStates.sprint];
    }
    public PlayerBaseState Strafe()
    {
        return _states[PlayerStates.strafe];
    }
}
