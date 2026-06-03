using UnityEngine;

public class WalkState : IState
{
    private readonly BotStateMachine _stateMachine;
    private Vector3 _target;

    public WalkState(BotStateMachine stateMachine) =>    
        _stateMachine = stateMachine;
    
    public void Enter()
    {

        if (_stateMachine.Bot.TargetResource != null && _stateMachine.Bot.Inventory.IsFull == false)
        {
            _target = _stateMachine.Bot.TargetResource.transform.position;
        }
        else if (_stateMachine.Bot.Inventory.IsFull) 
        {
            _target = _stateMachine.Bot.OwnerBasePosition;
        }

        _stateMachine.Bot.Mover.MoveTo(_target);
    }

    public void Exit() =>
        _stateMachine.Bot.Mover.Stop();

    public void Update()
    {
        if (_stateMachine.Bot.Mover.IsMovingComplete() && _stateMachine.Bot.Inventory.IsFull == false)
        {
            _stateMachine.TransitionTo<GatheringState>();
        }
        else if (_stateMachine.Bot.Mover.IsMovingComplete() && _stateMachine.Bot.Inventory.IsFull)
        {
            _stateMachine.TransitionTo<DropState>();
        }
    }
}