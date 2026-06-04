using System;

public class BaseStateMachine : StateMachine
{
    public IBase Base { get; private set; }

    public void Init(IBase tbase)
    {
        Base = tbase;

        AddState<NormalState>(new NormalState(this));
        AddState<ExpandState>(new ExpandState(this));

        TransitionTo<NormalState>();
    }
}