using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(GrassPainter))]
public class GrassRenderer : MonoBehaviour
{
    [SerializeField] private Mesh _bladeMesh;
    [SerializeField] private Material _grassMaterial;
    [SerializeField] private float _maxViewDistance = 50;

    [Header("Trample")]
    [SerializeField] private float _bendRadius = 2f;
    [SerializeField] private float _bendStrength = 1f;
    [SerializeField] private LayerMask _interactorLayers = 1;
    [SerializeField] private int _maxColliders = 16;

    private GrassPainter _painter;
    private MaterialPropertyBlock _props;
    private Bounds _renderBounds;

    private ComputeBuffer _colliderPosBuffer;
    private ComputeBuffer _colliderVelBuffer;
    private Vector4[] _colliderPositions;
    private Vector4[] _colliderVelocities;
    private Collider[] _colliderResults;
    private Dictionary<int, Vector3> _prevPositions;

    private int _positionsId;
    private int _colliderPositionsId;
    private int _colliderVelocitiesId;
    private int _colliderCountId;
    private int _bendRadiusId;
    private int _bendStrengthId;

    private void Awake()
    {
        _painter = GetComponent<GrassPainter>();
        _props = new MaterialPropertyBlock();

        if (_bladeMesh == null)
            _bladeMesh = CreateTriangleMesh();

        _renderBounds = new Bounds(transform.position, Vector3.one * _maxViewDistance * 2);

        int stride = sizeof(float) * 4;
        int bufSize = Mathf.Max(1, _maxColliders);
        _colliderPosBuffer = new ComputeBuffer(bufSize, stride, ComputeBufferType.Structured);
        _colliderVelBuffer = new ComputeBuffer(bufSize, stride, ComputeBufferType.Structured);
        _colliderPositions = new Vector4[bufSize];
        _colliderVelocities = new Vector4[bufSize];
        _colliderResults = new Collider[bufSize];
        _prevPositions = new Dictionary<int, Vector3>(bufSize);

        _positionsId = Shader.PropertyToID("_Positions");
        _colliderPositionsId = Shader.PropertyToID("_ColliderPositions");
        _colliderVelocitiesId = Shader.PropertyToID("_ColliderVelocities");
        _colliderCountId = Shader.PropertyToID("_ColliderCount");
        _bendRadiusId = Shader.PropertyToID("_BendRadius");
        _bendStrengthId = Shader.PropertyToID("_BendStrength");
    }

    private void Update()
    {
        if (_painter.PositionsBuffer == null || _grassMaterial == null)
            return;

        _props.SetBuffer(_positionsId, _painter.PositionsBuffer);

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position, _bendRadius * 3, _colliderResults, _interactorLayers);
        int actualCount = Mathf.Min(hitCount, _maxColliders);

        float dt = Mathf.Max(Time.deltaTime, 0.001f);

        for (int i = 0; i < actualCount; i++)
        {
            Collider col = _colliderResults[i];
            int id = col.GetInstanceID();
            Vector3 worldPos = col.transform.position;

            Vector3 velocity = Vector3.zero;
            if (_prevPositions.TryGetValue(id, out Vector3 prevPos))
                velocity = (worldPos - prevPos) / dt;

            _prevPositions[id] = worldPos;

            _colliderPositions[i] = new Vector4(worldPos.x, worldPos.y, worldPos.z, 0);
            _colliderVelocities[i] = new Vector4(velocity.x, velocity.y, velocity.z, 0);
        }

        for (int i = actualCount; i < _maxColliders; i++)
        {
            _colliderPositions[i] = Vector4.zero;
            _colliderVelocities[i] = Vector4.zero;
        }

        if (Time.frameCount % 120 == 0)
            _prevPositions.Clear();

        _colliderPosBuffer.SetData(_colliderPositions, 0, 0, _maxColliders);
        _colliderVelBuffer.SetData(_colliderVelocities, 0, 0, _maxColliders);

        _props.SetBuffer(_colliderPositionsId, _colliderPosBuffer);
        _props.SetBuffer(_colliderVelocitiesId, _colliderVelBuffer);
        _props.SetInt(_colliderCountId, actualCount);
        _props.SetFloat(_bendRadiusId, _bendRadius);
        _props.SetFloat(_bendStrengthId, _bendStrength);

        Graphics.DrawMeshInstancedProcedural(
            _bladeMesh,
            0,
            _grassMaterial,
            _renderBounds,
            _painter.BladeCount,
            _props,
            ShadowCastingMode.On,
            true,
            gameObject.layer
        );
    }

    private void OnDestroy()
    {
        _colliderPosBuffer?.Release();
        _colliderVelBuffer?.Release();
    }

    private static Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            Vector3.zero,
            Vector3.right * 0.01f,
            Vector3.forward * 0.01f
        };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 0.01f);
        return mesh;
    }
}
