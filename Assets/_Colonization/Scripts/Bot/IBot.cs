using UnityEngine;

public interface IBot
{
    Vector3 OwnerBasePosition { get; }
    Resource TargetResource { get; }
    bool HasConstructTask { get; set; }
    IMover Mover { get; }
    IInventory Inventory { get; }
    void GiveResource(Resource resource);
    void SetResourceToMine(Resource resource);
}