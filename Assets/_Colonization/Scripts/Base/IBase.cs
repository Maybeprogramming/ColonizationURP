using UnityEngine;

public interface IBase
{
    int ResourceCount { get; }
    int BotCount { get; }
    bool HasConstructNewBase { get; }
    bool HasBotOnConstructTask { get; }
    Vector3 FlagPosition { get; }
    BaseFactory BaseFactory { get; }
    bool TrySpawnBot();
    bool TrySpendResources(int count);
    Bot GetFreeBot();
    void AddBot(Bot bot);
    void RemoveBot(Bot bot);
    void CancelConstructTasks();
    void AssignExpansionFlag(Vector3 position);
    void CancelExpansion();
    void ClearExpansionFlag();
}
