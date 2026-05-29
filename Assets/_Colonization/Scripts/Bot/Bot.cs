using UnityEngine;

[RequireComponent(typeof(Mover), 
                  typeof(BotStateMachine),
                  typeof(Inventory))]

public class Bot: MonoBehaviour, IBot
{
    [SerializeField] private Base _base;
    [SerializeField] private Resource _currentResource;

    private Mover _mover;
    private Inventory _resourceContainer;
    private BotStateMachine _stateMachine;

    public Resource CurrentResource => _currentResource;

    public Vector3 CurrentBasePosition => _base.transform.position;

    public bool IsBusy => _stateMachine.GetCurrentState is IdleState == false;

    private void Start()
    {
        _mover = GetComponent<Mover>();
        _resourceContainer = GetComponent<Inventory>();
        _stateMachine = GetComponent<BotStateMachine>();
        _stateMachine.Init(this, _mover, _resourceContainer);
    }

    public void GiveResource(Resource resource)
    {
        _base.TakeResource(resource);
        _currentResource = null;
    }

    public void SetResourceToMine(Resource resource) =>    
        _currentResource = resource;    
}