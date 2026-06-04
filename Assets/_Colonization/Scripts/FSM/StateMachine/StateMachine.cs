using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour, IStateMachine
{
    private IState _currentState;
    private Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

    public event Action<Type> StateChanged;

    public void Update() =>
        UpdateState();

    protected virtual void UpdateState() =>
        _currentState?.Update();

    public IState CurrentState => _currentState;

    public void TransitionTo<T>() where T : IState
    {
        _currentState?.Exit();

        var type = typeof(T);

        if (_states.TryGetValue(type, out IState state))
        {
            _currentState = state;
            _currentState.Enter();

            StateChanged?.Invoke(typeof(T));
        }
        else
        {
            Debug.LogError($"Состояние {type.Name} не найдено!");
        }
    }

    protected void AddState<T>(IState state) where T : IState =>    
        _states.Add(typeof(T), state);    
}