public interface IInventory
{
    bool IsFull { get; }

    void Add(Resource resource);
    Resource Drop();
}