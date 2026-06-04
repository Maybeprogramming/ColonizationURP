using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnerResources : BaseResourcePool<Resource>
{
    [SerializeField] private Transform _container;
    [SerializeField] private float _minTimeSpawn;
    [SerializeField] private float _maxTimeSpawn;
    [SerializeField] private float _offsetXPosition;
    [SerializeField] private float _offsetZPosition;
    [SerializeField] private float _positionY;
    [SerializeField] private int _maxCount;

    public Vector3 SpawnCenterPosition => transform.position;
    public event Action Spawned;

    private float _nextSpawnTime;

    private void Start()
    {
        PoolInit();
        _nextSpawnTime = Time.time + Random.Range(_minTimeSpawn, _maxTimeSpawn);
        StartCoroutine(Spawning());
    }

    public void ResourceReleasedHandler(Resource resource)
    {
        resource.transform.parent = _container.transform;
        Pool.Release(resource);
    }

    private void Spawn()
    {
        Vector3 newSpawnPosition = GetRandomPosition();

        Resource resource = Pool.Get();
        resource.transform.position = newSpawnPosition;
        resource.transform.rotation = Quaternion.identity;
        resource.transform.localScale = Vector3.one;
        resource.transform.parent = _container.transform;

        Spawned?.Invoke();
    }

    private Vector3 GetRandomPosition()
    {
        float minX = SpawnCenterPosition.x - _offsetXPosition;
        float maxX = SpawnCenterPosition.x + _offsetXPosition;
        float minZ = SpawnCenterPosition.z - _offsetZPosition;
        float maxZ = SpawnCenterPosition.z + _offsetZPosition;

        return new Vector3(Random.Range(minX, maxX), _positionY, Random.Range(minZ, maxZ));
    }

    private IEnumerator Spawning()
    {
        while (enabled)
        {
            if (Time.time >= _nextSpawnTime)
            {
                if (Pool.CountActive < _maxCount)
                    Spawn();

                _nextSpawnTime = Time.time + Random.Range(_minTimeSpawn, _maxTimeSpawn);
            }

            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = SpawnCenterPosition;
        Vector3 size = new Vector3(_offsetXPosition + _offsetXPosition, 1f, _offsetZPosition + _offsetZPosition);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
