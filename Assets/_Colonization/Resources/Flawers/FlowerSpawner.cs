using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    [SerializeField] private int _flowerCount = 2000;
    [SerializeField] private Bounds _spawnBounds = new Bounds(Vector3.zero, Vector3.one * 10);
    [SerializeField] private LayerMask _groundLayer = ~0;

    private ComputeBuffer _positionsBuffer;
    private Vector4[] _positions;

    public ComputeBuffer PositionsBuffer => _positionsBuffer;
    public int FlowerCount => _flowerCount;

    private void Awake()
    {
        GeneratePositions();
    }

    private void GeneratePositions()
    {
        _positions = new Vector4[_flowerCount];

        for (int i = 0; i < _flowerCount; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
                _spawnBounds.center.y,
                Random.Range(_spawnBounds.min.z, _spawnBounds.max.z)
            );

            RaycastHit hit;

            if (Physics.Raycast(new Vector3(randomPoint.x, _spawnBounds.max.y + 10, randomPoint.z),
                                Vector3.down, out hit, 50, _groundLayer))
            {
                randomPoint.y = hit.point.y;
            }
            else
            {
                randomPoint.y = 0;
            }

            _positions[i] = new Vector4(randomPoint.x, randomPoint.y, randomPoint.z, Random.value);
        }

        _positionsBuffer = new ComputeBuffer(_flowerCount, 16, ComputeBufferType.Structured);
        _positionsBuffer.SetData(_positions);
    }

    private void OnDestroy()
    {
        _positionsBuffer?.Release();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
    }
}
