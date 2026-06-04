using UnityEngine;

[RequireComponent(typeof(Mover),
                  typeof(BotStateMachine),
                  typeof(Inventory))]
public class Bot : MonoBehaviour, IBot
{
    [SerializeField] private Base _ownerBase;
    [SerializeField] private Resource _targetResource;

    private Mover _mover;
    private Inventory _botInventory;
    private BotStateMachine _stateMachine;
    private BaseFactory _cachedBaseFactory;

    [field: SerializeField] public bool HasConstructTask { get; set; }
    public Vector3 ConstructTargetPosition { get; set; }

    public Resource TargetResource => _targetResource;
    public Vector3 OwnerBasePosition => _ownerBase.transform.position;
    public IBase OwnerBase => _ownerBase;
    public bool IsBusy => _stateMachine.CurrentState?.IsBusy ?? false;
    public IInventory Inventory => _botInventory;
    public IMover Mover => _mover;

    private void Start()
    {
        _mover = GetComponent<Mover>();
        _botInventory = GetComponent<Inventory>();
        _stateMachine = GetComponent<BotStateMachine>();
        _stateMachine.SetOwnerBase(this, _cachedBaseFactory);
    }

    public void GiveResource(Resource resource)
    {
        _ownerBase.TakeResource(resource);
        _targetResource = null;
    }

    public void SetTargetResource(Resource resource) =>
        _targetResource = resource;

    public void Init(Base ownerBase, BaseFactory baseFactory)
    {
        _ownerBase = ownerBase;
        _cachedBaseFactory = baseFactory;
    }

    public void SwitchBase(IBase newBase)
    {
        if (newBase == null)
            return;

        _ownerBase.RemoveBot(this);
        _ownerBase = (Base)newBase;
        newBase.AddBot(this);
        HasConstructTask = false;
        _targetResource = null;

        if (_stateMachine != null && newBase.BaseFactory != null)
            _stateMachine.SetOwnerBase(this, newBase.BaseFactory);
    }
}
