using UnityEngine;

public class ExpandState : IState
{
    private const int ExpandCost = 5;

    private BaseStateMachine _stateMachine;

    public ExpandState(BaseStateMachine baseStateMachine)
    {
        _stateMachine = baseStateMachine;
    }

    public void Enter()
    {
        IBase baseData = _stateMachine.Base;

        if (baseData.BotCount <= 1)
        {
            _stateMachine.TransitionTo<NormalState>();
            return;
        }

        Bot freeBot = FindFreeBot();

        if (freeBot == null)
        {
            _stateMachine.TransitionTo<NormalState>();
            return;
        }

        if (baseData.TrySpendResources(ExpandCost) == false)
        {
            _stateMachine.TransitionTo<NormalState>();
            return;
        }

        freeBot.ConstructTargetPosition = baseData.FlagPosition;
        freeBot.HasConstructTask = true;
    }

    public void Exit() { }

    public void Update()
    {
        if (_stateMachine.Base.HasConstractNewBase == false)
            _stateMachine.TransitionTo<NormalState>();
    }

    private Bot FindFreeBot() =>
        _stateMachine.Base.GetFreeBot();
}
