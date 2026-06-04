using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ButterflySpawner : MonoBehaviour
{
    private const float GizmoAlpha = 0.3f;

    [SerializeField] private Butterfly _butterflyPrefab;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private Bounds _spawnBounds;
    [SerializeField] private float _minimumHeight;
    [SerializeField] private float _maximumHeight;
    [SerializeField] private float _minimumSpeed;
    [SerializeField] private float _maximumSpeed;
    [SerializeField] private float _minimumLifetime;
    [SerializeField] private float _maximumLifetime;
    [SerializeField] private int _maximumButterflies;
    [SerializeField] private float _spawnInterval;
    [SerializeField] private int _initialCount;
    [SerializeField] private float _globalScale;

    private ObjectPool<Butterfly> _pool;
    private List<Butterfly> _activeButterflies;
    private WaitForSeconds _spawnIntervalWait;

    private void Awake()
    {
        _activeButterflies = new List<Butterfly>();
        _spawnIntervalWait = new WaitForSeconds(_spawnInterval);

        _pool = new ObjectPool<Butterfly>(
            createFunc: CreateButterfly,
            actionOnGet: GetButterfly,
            actionOnRelease: ReleaseButterfly,
            actionOnDestroy: DestroyButterfly,
            collectionCheck: false,
            defaultCapacity: _maximumButterflies,
            maxSize: _maximumButterflies
        );
    }

    private void Start()
    {
        for (int i = 0; i < _initialCount; i++)
            Spawn();

        StartCoroutine(SpawnRoutine());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, GizmoAlpha);
        Gizmos.DrawCube(_spawnBounds.center, _spawnBounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
    }

    private Butterfly CreateButterfly()
    {
        Butterfly butterfly = Instantiate(_butterflyPrefab, transform);
        butterfly.Died += OnButterflyDied;
        return butterfly;
    }

    private void DestroyButterfly(Butterfly butterfly)
    {
        butterfly.Died -= OnButterflyDied;
        Destroy(butterfly.gameObject);
    }

    private void GetButterfly(Butterfly butterfly)
    {
        butterfly.gameObject.SetActive(true);
        _activeButterflies.Add(butterfly);
    }

    private void ReleaseButterfly(Butterfly butterfly)
    {
        butterfly.gameObject.SetActive(false);
        _activeButterflies.Remove(butterfly);
    }

    private void OnButterflyDied(Butterfly butterfly)
    {
        _pool.Release(butterfly);
    }

    private void Spawn()
    {
        if (_activeButterflies.Count >= _maximumButterflies)
            return;

        if (_sprites == null || _sprites.Length == 0)
        {
            Debug.LogWarning($"{name}: массив спрайтов пуст. Назначьте спрайты в инспекторе.");
            return;
        }

        Butterfly butterfly = _pool.Get();

        Vector3 position = new Vector3(
            Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
            Random.Range(_minimumHeight, _maximumHeight),
            Random.Range(_spawnBounds.min.z, _spawnBounds.max.z)
        );

        butterfly.transform.position = position;
        butterfly.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        Sprite sprite = _sprites[Random.Range(0, _sprites.Length)];
        float speed = Random.Range(_minimumSpeed, _maximumSpeed);
        float lifetime = Random.Range(_minimumLifetime, _maximumLifetime);

        butterfly.Initialize(sprite, _spawnBounds, _minimumHeight, _maximumHeight, speed, lifetime, _globalScale);
    }

    private IEnumerator SpawnRoutine()
    {
        while (enabled)
        {
            yield return _spawnIntervalWait;
            Spawn();
        }
    }
}
