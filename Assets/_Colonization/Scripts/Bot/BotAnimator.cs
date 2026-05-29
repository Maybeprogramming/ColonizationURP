using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(BotStateMachine))]
public class BotAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private BotStateMachine _botStateMachine;

    private Dictionary<Type, string> _animations;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _botStateMachine = GetComponent<BotStateMachine>();

        _animations = new Dictionary<Type, string>()
        {
            {typeof(IdleState), nameof(IdleState) },
            {typeof(WalkState) , nameof(WalkState) },
            {typeof (GatheringState), nameof(GatheringState) },
        };
    }

    private void OnEnable() 
    {
        _botStateMachine.StateChanged += OnChangedState;
    }

    private void OnDisable()
    {
        _botStateMachine.StateChanged -= OnChangedState;
    }    

    private void OnChangedState(Type type)
    {
        if (_animations.TryGetValue(type, out string animation))
        {
            _animator.Play(animation);
        }
        else
        {
            _animator.Play(nameof(IdleState));
        }
    }
}