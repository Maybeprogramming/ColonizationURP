using UnityEngine;

public class BotFactory : MonoBehaviour
{
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private Base _base;

    [SerializeField] private Transform _spawnTransform;

    private Vector3 Position => _spawnTransform != null ? _spawnTransform.position : _base.transform.position;

    public void Initialize(Base ownerBase)
    {
        _base = ownerBase;
    }

    public void Spawn()
    {
        _base.AddBot(CreateNewBot());
    }

    private Bot CreateNewBot()
    {
        Bot bot = Instantiate(_botPrefab, Position, Quaternion.identity);
        bot.Init(_base);

        return bot;
    }
}
