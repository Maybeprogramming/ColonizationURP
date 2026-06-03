using System;

public class BaseStateMachine : StateMachine
{
    public IBase Base { get; private set; }

    private void Start()
    {
        AddState<NormalState>(new NormalState(this));
        AddState<ExpandState>(new ExpandState(this));

        TransitionTo<NormalState>();
    }

    public void Init(IBase tbase)
    {
        Base = tbase;
    }
}