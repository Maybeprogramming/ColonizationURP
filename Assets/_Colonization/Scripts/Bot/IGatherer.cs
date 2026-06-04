public interface IGatherer
{
    Resource TargetResource { get; }
    IInventory Inventory { get; }
    void GiveResource(Resource resource);
    void SetTargetResource(Resource resource);
}
