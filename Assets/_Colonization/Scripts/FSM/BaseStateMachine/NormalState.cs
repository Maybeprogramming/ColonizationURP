public class NormalState : IState
{
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

        if (baseData.HasConstructNewBase == false && baseData.ResourceCount >= BaseBalance.BotSpawnCost)
        {
            baseData.TrySpawnBot();
            return;
        }

        if (baseData.HasConstructNewBase == false)
            return;

        if (baseData.ResourceCount >= BaseBalance.ExpandCost &&
            baseData.BotCount > 1 &&
            baseData.HasBotOnConstructTask == false)
        {
            _stateMachine.TransitionTo<ExpandState>();
        }
    }
}
