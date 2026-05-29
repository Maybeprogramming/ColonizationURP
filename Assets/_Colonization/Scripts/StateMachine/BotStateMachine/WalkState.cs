using UnityEngine;

public class WalkState : IState
{
    private readonly BotStateMachine _stateMachine;
    private Vector3 _target;
    private IMover _mover;

    public WalkState(BotStateMachine stateMachine) =>    
        _stateMachine = stateMachine;
    
    public void Enter()
    {
        _mover = _stateMachine.Mover;

        if (_stateMachine.Bot.CurrentResource != null && _stateMachine.Inventory.IsFull == false)
        {
            _target = _stateMachine.Bot.CurrentResource.transform.position;
        }
        else if (_stateMachine.Inventory.IsFull) 
        {
            _target = _stateMachine.Bot.CurrentBasePosition;
        }

        _mover.MoveTo(_target);
    }

    public void Exit() =>    
        _mover.Stop();

    public void Update()
    {
        if (_mover.IsMovingComplete() && _stateMachine.Inventory.IsFull == false)
        {
            _stateMachine.TransitionTo<GatheringState>();
        }
        else if (_mover.IsMovingComplete() && _stateMachine.Inventory.IsFull)
        {
            _stateMachine.TransitionTo<DropState>();
        }
    }
}