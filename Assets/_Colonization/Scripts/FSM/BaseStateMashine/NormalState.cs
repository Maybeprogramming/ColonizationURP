using UnityEngine;

public class NormalState : IState
{
    private const int BotSpawnCost = 3;
    private const int ExpandCost = 5;

    private BaseStateMachine _stateMachine;

    public NormalState(BaseStateMachine baseStateMachine)
    {
        _stateMachine = baseStateMachine;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update()
    {
        IBase baseData = _stateMachine.Base;

        if (baseData.HasConstractNewBase == false && baseData.ResourceCount >= BotSpawnCost)
        {
            baseData.TrySpawnBot();
            return;
        }

        if (baseData.HasConstractNewBase == false)
            return;

        if (baseData.ResourceCount >= ExpandCost &&
            baseData.BotCount > 1 &&
            baseData.HasBotOnConstructTask == false)
        {
            _stateMachine.TransitionTo<ExpandState>();
        }
    }
}
