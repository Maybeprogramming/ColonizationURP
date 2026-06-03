using UnityEngine;

public class ConstructState : IState
{
    private float _animationDuration;
    private BotStateMachine _stateMachine;

    public ConstructState(BotStateMachine botStateMachine) =>    
        _stateMachine = botStateMachine;

    public void Enter()
    {
        _animationDuration = 3f;
    }

    public void Exit()
    {
        _stateMachine.Bot.HasConstructTask = false;
    }

    public void Update()
    {
        _animationDuration -= Time.deltaTime;

        if (_animationDuration <= 0)
        {        
            _stateMachine.TransitionTo<IdleState>();
        }
    }
}
