using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ButterflySpawner : MonoBehaviour
{
    [SerializeField] private Butterfly _butterflyPrefab;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Bounds _spawnBounds;
    [SerializeField] private float _minHeight;
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _minLifetime;
    [SerializeField] private float _maxLifetime;
    [SerializeField] private int _maxButterflies;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private int _initialCount;
    [SerializeField] private float _globalScale;

    private ObjectPool<Butterfly> _pool;
    private List<Butterfly> _active;

    private void Awake()
    {
        _active = new List<Butterfly>();
        _pool = new ObjectPool<Butterfly>(
            createFunc: CreateButterfly,
            actionOnGet: OnGetButterfly,
            actionOnRelease: OnReleaseButterfly,
            actionOnDestroy: b =>
            {
                b.OnDeath -= OnButterflyDeath;
                Destroy(b.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: _maxButterflies,
            maxSize: _maxButterflies
        );
    }

    private void Start()
    {
        for (int i = 0; i < _initialCount; i++)
            Spawn();

        StartCoroutine(SpawnRoutine());
    }

    private Butterfly CreateButterfly()
    {
        Butterfly b = Instantiate(_butterflyPrefab, transform);
        b.OnDeath += OnButterflyDeath;
        return b;
    }

    private void OnGetButterfly(Butterfly b)
    {
        b.gameObject.SetActive(true);
        _active.Add(b);
    }

    private void OnReleaseButterfly(Butterfly b)
    {
        b.gameObject.SetActive(false);
        _active.Remove(b);
    }

    private void OnButterflyDeath(Butterfly b)
    {
        _pool.Release(b);
    }

    private void Spawn()
    {
        if (_active.Count >= _maxButterflies)
            return;

        if (_sprites == null || _sprites.Length == 0)
        {
            Debug.LogWarning($"{name}: массив спрайтов пуст. Назначьте спрайты в инспекторе.");
            return;
        }

        Butterfly b = _pool.Get();

        Vector3 pos = new Vector3(
            Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
            Random.Range(_minHeight, _maxHeight),
            Random.Range(_spawnBounds.min.z, _spawnBounds.max.z)
        );

        b.transform.position = pos;
        b.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        Sprite sprite = _sprites[Random.Range(0, _sprites.Length)];
        float speed = Random.Range(_minSpeed, _maxSpeed);
        float lifetime = Random.Range(_minLifetime, _maxLifetime);

        b.Initialize(sprite, _spawnBounds, _minHeight, _maxHeight, speed, lifetime, _globalScale);
    }

    private IEnumerator SpawnRoutine()
    {
        WaitForSeconds interval = new WaitForSeconds(_spawnInterval);

        while (enabled)
        {
            yield return interval;
            Spawn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Gizmos.DrawCube(_spawnBounds.center, _spawnBounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
    }
}
