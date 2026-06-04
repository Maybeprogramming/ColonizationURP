using UnityEngine;

public class GrassPainter : MonoBehaviour
{
    private const int Stride = 16;
    private const float RaycastOffset = 10f;
    private const float RaycastMaxDistance = 50f;

    [SerializeField] private int _bladeCount = 5000;
    [SerializeField] private Bounds _spawnBounds = new Bounds(Vector3.zero, Vector3.one * 10);
    [SerializeField] private LayerMask _groundLayer = ~0;

    private ComputeBuffer _positionsBuffer;
    private Vector4[] _positions;

    public ComputeBuffer PositionsBuffer => _positionsBuffer;
    public int BladeCount => _bladeCount;

    private void Awake()
    {
        GeneratePositions();
    }

    private void OnDestroy()
    {
        _positionsBuffer?.Release();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_spawnBounds.center, _spawnBounds.size);
    }

    private void GeneratePositions()
    {
        _positions = new Vector4[_bladeCount];

        for (int i = 0; i < _bladeCount; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
                _spawnBounds.center.y,
                Random.Range(_spawnBounds.min.z, _spawnBounds.max.z)
            );

            Vector3 rayOrigin = new Vector3(randomPoint.x, _spawnBounds.max.y + RaycastOffset, randomPoint.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RaycastMaxDistance, _groundLayer))
            {
                randomPoint.y = hit.point.y;
            }
            else
            {
                randomPoint.y = 0;
            }

            _positions[i] = new Vector4(randomPoint.x, randomPoint.y, randomPoint.z, Random.value);
        }

        _positionsBuffer = new ComputeBuffer(_bladeCount, Stride, ComputeBufferType.Structured);
        _positionsBuffer.SetData(_positions);
    }
}
