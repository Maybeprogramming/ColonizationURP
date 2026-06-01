using UnityEngine;

public class BotFactory : MonoBehaviour
{
    [SerializeField] private Bot _botPrefab;
    [SerializeField] private Base _base;

    [SerializeField] private Transform _transform;

    private Vector3 Position => _transform.position;

    [ContextMenu("Instance/create")]
    private void CreateBot()
    {
        var bot = Instantiate(_botPrefab, Position, Quaternion.identity);
        bot.Init(_base);
    }
}