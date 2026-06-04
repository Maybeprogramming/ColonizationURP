using System.Collections.Generic;

public class BotRoster
{
    private readonly List<Bot> _bots;

    public BotRoster(List<Bot> initialBots)
    {
        _bots = initialBots ?? new List<Bot>();
    }

    public int Count => _bots.Count;
    public IReadOnlyList<Bot> Bots => _bots;

    public void Add(Bot bot)
    {
        if (bot == null)
            return;

        _bots.Add(bot);
    }

    public void Remove(Bot bot) =>
        _bots.Remove(bot);

    public bool Contains(Bot bot) =>
        bot != null && _bots.Contains(bot);

    public bool HasOnConstructTask() =>
        _bots.Exists(bot => bot != null && bot.HasConstructTask);

    public Bot GetFreeBot() =>
        _bots.Find(bot => bot != null && bot.IsBusy == false && bot.HasConstructTask == false);

    public void CancelConstructTasks()
    {
        for (int i = 0; i < _bots.Count; i++)
        {
            if (_bots[i] != null)
                _bots[i].HasConstructTask = false;
        }
    }

    public void ClearNulls() =>
        _bots.RemoveAll(bot => bot == null);
}
