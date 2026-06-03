using UnityEngine;

public class ExpandState : IState
{
    private BaseStateMachine _stateMachine;

    public ExpandState(BaseStateMachine baseStateMachine)
    {
        _stateMachine = baseStateMachine;
    }

    public void Enter()
    {
        Debug.Log($"Состояние постройки новой базы");
    }

    public void Exit()
    {
        _stateMachine.Base.HasConstractNewBase = false;
        Debug.Log($"База построена");
    }

    public void Update()
    {
        if (_stateMachine.Base.ResourceCount > 5)
        {            
            _stateMachine.TransitionTo<NormalState>();
        }
    }
}
