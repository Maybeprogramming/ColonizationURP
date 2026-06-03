using UnityEngine;

public class NormalState : IState
{
    private BaseStateMachine _stateMachine;

    public NormalState(BaseStateMachine baseStateMachine)
    {
        _stateMachine = baseStateMachine;
    }

    public void Enter()
    {
        Debug.Log($"Состояние производства юнитов");
    }

    public void Exit()
    {

    }

    public void Update()
    {
        if (_stateMachine.Base.ResourceCount > 3 && _stateMachine.Base.HasConstractNewBase == false)
        {
            Debug.Log($"Произвести юнита, потратив 3 ресурса");
        }
        else if(_stateMachine.Base.ResourceCount > 5 && _stateMachine.Base.HasConstractNewBase)
        {
            Debug.Log($"Назначить юнита для постройки новой базы, потратив 5 ресурсов");
            _stateMachine.TransitionTo<ExpandState>();
        }
    }
}
