public class DropState : IState
{
    private readonly BotStateMachine _stateMachine;

    public DropState(BotStateMachine stateMachine) =>    
        _stateMachine = stateMachine;

    public void Enter() =>    
        _stateMachine.Bot.GiveResource(_stateMachine.Bot.Inventory.Drop());    

    public void Exit() { } 

    public void Update()
    {
        if (_stateMachine.Bot.Inventory.IsFull == false)
        { 
            _stateMachine.Bot.SetResourceToMine(null);
            _stateMachine.TransitionTo<IdleState>();
        }
    }
}