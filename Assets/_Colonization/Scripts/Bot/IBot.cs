using UnityEngine;

public interface IBot
{
    Vector3 OwnerBasePosition { get; }
    Resource TargetResource { get; }
    bool HasConstructTask { get; set; }
    Vector3 ConstructTargetPosition { get; set; }
    IMover Mover { get; }
    IInventory Inventory { get; }
    IBase OwnerBase { get; }
    void GiveResource(Resource resource);
    void SetTargetResource(Resource resource);
    void SwitchBase(IBase newBase);
}
