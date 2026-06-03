using UnityEngine;

public class GatheringState : IState
{
    private readonly BotStateMachine _stateMachine;

    private float _delayTimer;

    public GatheringState(BotStateMachine stateMachine) =>
        _stateMachine = stateMachine;

    public void Enter()
    {
        _delayTimer = 1.5f;
    }

    private void PickupResource()
    {
        if (_stateMachine.Bot.TargetResource != null && _stateMachine.Bot.Inventory.IsFull == false)
            _stateMachine.Bot.Inventory.Add(_stateMachine.Bot.TargetResource);
    }

    public void Exit() { }

    public void Update()
    {
        _delayTimer -= Time.deltaTime;

        if (_delayTimer <= 0)
        {
            PickupResource();

            if (_stateMachine.Bot.Inventory.IsFull)
            {
                _stateMachine.TransitionTo<WalkState>();
            }
        }
    }
}