using UnityEngine;

public class WalkState : IState
{
    private readonly BotStateMachine _stateMachine;
    private Vector3 _target;

    public WalkState(BotStateMachine stateMachine) =>
        _stateMachine = stateMachine;

    public void Enter()
    {
        IBot bot = _stateMachine.Bot;

        if (bot.HasConstructTask)
        {
            _target = bot.ConstructTargetPosition;
        }
        else if (bot.TargetResource != null && bot.Inventory.IsFull == false)
        {
            _target = bot.TargetResource.transform.position;
        }
        else if (bot.Inventory.IsFull)
        {
            _target = bot.OwnerBasePosition;
        }

        bot.Mover.MoveTo(_target);
    }

    public void Exit() =>
        _stateMachine.Bot.Mover.Stop();

    public void Update()
    {
        IBot bot = _stateMachine.Bot;

        if (bot.Mover.IsMovingComplete() == false)
            return;

        if (bot.HasConstructTask)
        {
            _stateMachine.TransitionTo<ConstructState>();
        }
        else if (bot.Inventory.IsFull == false)
        {
            _stateMachine.TransitionTo<GatheringState>();
        }
        else if (bot.Inventory.IsFull)
        {
            _stateMachine.TransitionTo<DropState>();
        }
        else
        {
            _stateMachine.TransitionTo<IdleState>();
        }
    }
}
