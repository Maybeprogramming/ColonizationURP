using UnityEngine;

[RequireComponent(typeof(Mover), 
                  typeof(BotStateMachine),
                  typeof(Inventory))]

public class Bot: MonoBehaviour, IBot
{
    [SerializeField] private Base _ownerBase;
    [SerializeField] private Resource _targetResource;

    private Mover _mover;
    private Inventory _botInventory;
    private BotStateMachine _stateMachine;

    [field: SerializeField] public bool HasConstructTask { get; set; }

    public Resource TargetResource => _targetResource;

    public Vector3 OwnerBasePosition => _ownerBase.transform.position;

    public bool IsBusy => _stateMachine.GetCurrentState is IdleState == false;

    public IInventory Inventory => _botInventory;
    public IMover Mover => _mover;

    private void Start()
    {
        _mover = GetComponent<Mover>();
        _botInventory = GetComponent<Inventory>();
        _stateMachine = GetComponent<BotStateMachine>();
        _stateMachine.Init(this);
    }

    public void GiveResource(Resource resource)
    {
        _ownerBase.TakeResource(resource);
        _targetResource = null;
    }

    public void SetResourceToMine(Resource resource) =>    
        _targetResource = resource;

    public void Init(Base cbase)
    {
        _ownerBase = cbase;
    }
}