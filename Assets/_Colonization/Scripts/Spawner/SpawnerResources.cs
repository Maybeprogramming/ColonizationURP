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
    [SerializeField] private Vector3 _spawnCenterPosition;
    [SerializeField] private int _maxCount;

    public event Action Spawned;

    private void Start()
    {
        PoolInit();
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
        float minX = _spawnCenterPosition.x - _offsetXPosition;
        float maxX = _spawnCenterPosition.x + _offsetXPosition;
        float minZ = _spawnCenterPosition.z - _offsetZPosition;
        float maxZ = _spawnCenterPosition.z + _offsetZPosition;

        return new Vector3(Random.Range(minX, maxX), _positionY, Random.Range(minZ, maxZ));
    }

    private IEnumerator Spawning()
    {
        while (enabled)
        {
            if (Pool.CountActive < _maxCount)
            {
                Spawn();
            }

            yield return GetRandomDelayTime();            
        }
    }

    private WaitForSeconds GetRandomDelayTime() =>    
        new WaitForSeconds(Random.Range(_minTimeSpawn, _maxTimeSpawn));    
}