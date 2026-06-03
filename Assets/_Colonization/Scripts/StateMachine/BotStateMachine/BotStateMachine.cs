public class BotStateMachine : StateMachine
{
    public IBot Bot { get; private set; }

    private void Start()
    {
        AddState<IdleState>(new IdleState(this));
        AddState<WalkState>(new WalkState(this));
        AddState<GatheringState>(new GatheringState(this));
        AddState<DropState>(new DropState(this));
        AddState<ConstructState>(new ConstructState(this));

        TransitionTo<IdleState>();
    }

    public void Init(IBot bot)
    {
        Bot = bot;
    }
}