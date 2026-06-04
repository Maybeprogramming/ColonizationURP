using UnityEngine;
using UnityEngine.Pool;

public class BaseResourcePool<T> : MonoBehaviour where T : Resource
{
    private int _id;

    [SerializeField] private T _prefab;
    [SerializeField] private bool _collectionCheck;
    [SerializeField] private int _defaultCapacity;
    [SerializeField] private int _poolMaxSize;
    [SerializeField] private string _entityName;

    public ObjectPool<T> Pool { get; protected set; }

    protected void PoolInit()
    {
        _id = 0;

        Pool = new ObjectPool<T>
            (
                () => Create(),
                (resource) => Get(resource),
                (resource) => Release(resource),
                (resource) => ToDestroy(resource),
                _collectionCheck,
                _defaultCapacity,
                _poolMaxSize
            );
    }

    private void ToDestroy(T resource) =>
        GameObject.Destroy(resource);

    private void Release(T resource) =>
        resource.gameObject.SetActive(false);

    private void Get(T resource) =>
        resource.gameObject.SetActive(true);

    public T Create()
    {
        T instance = Instantiate(_prefab, Vector3.zero, Quaternion.identity);
        instance.gameObject.name = $"{_entityName}_{++_id}";

        return instance;
    }
}