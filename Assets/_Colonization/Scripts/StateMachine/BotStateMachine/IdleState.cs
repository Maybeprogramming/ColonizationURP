using UnityEngine;

public class IdleState : IState
{
    private readonly BotStateMachine _stateMachine;

    public IdleState(BotStateMachine stateMachine) =>    
        _stateMachine = stateMachine;

    public void Enter() { }

    public void Exit() { } 

    public void Update()
    {
        if(_stateMachine.Bot.CurrentResource != null)        
            _stateMachine.TransitionTo<WalkState>();        
    }
}