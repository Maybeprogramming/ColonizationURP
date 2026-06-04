using System;
using UnityEngine;

public class BotFactory : MonoBehaviour
{
    [SerializeField] private Bot _botPrefab;

    public event Action<Bot> BotCreated;

    public void Spawn()
    {
        Bot bot = Instantiate(_botPrefab);
        BotCreated?.Invoke(bot);
    }
}
