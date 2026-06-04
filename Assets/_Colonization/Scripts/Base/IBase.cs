using UnityEngine;

public interface IBase
{
    int ResourceCount { get; }
    int BotCount { get; }
    bool HasConstractNewBase { get; set; }
    bool HasBotOnConstructTask { get; }
    Vector3 FlagPosition { get; set; }
    bool TrySpawnBot();
    bool TrySpendResources(int count);
    Bot GetFreeBot();
    void CancelConstructTasks();
}
