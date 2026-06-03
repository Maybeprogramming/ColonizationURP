public class IdleState : IState
{
    private readonly BotStateMachine _stateMachine;

    public IdleState(BotStateMachine stateMachine) =>    
        _stateMachine = stateMachine;

    public void Enter() { }

    public void Exit() { } 

    public void Update()
    {
        if (_stateMachine.Bot.HasConstructTask)
            _stateMachine.TransitionTo<ConstructState>();

        if(_stateMachine.Bot.TargetResource != null)        
            _stateMachine.TransitionTo<WalkState>();        
    }
}