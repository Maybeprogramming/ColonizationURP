using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(FlowerSpawner))]
public class FlowerRenderer : MonoBehaviour
{
    private const float MeshVertexOffset = 0.01f;
    private const float BoundsMultiplier = 2f;

    private readonly int _positionsPropertyId = Shader.PropertyToID("_Positions");

    [SerializeField] private Material _flowerMaterial;
    [SerializeField] private float _maxViewDistance = 50;

    private FlowerSpawner _spawner;
    private MaterialPropertyBlock _propertyBlock;
    private Bounds _renderBounds;
    private Mesh _pointMesh;

    private void Awake()
    {
        _spawner = GetComponent<FlowerSpawner>();
        _propertyBlock = new MaterialPropertyBlock();

        _pointMesh = CreatePointMesh();
        _renderBounds = new Bounds(transform.position, Vector3.one * _maxViewDistance * BoundsMultiplier);
    }

    private void Update()
    {
        if (_spawner.PositionsBuffer == null || _flowerMaterial == null)
            return;

        _propertyBlock.SetBuffer(_positionsPropertyId, _spawner.PositionsBuffer);

        Graphics.DrawMeshInstancedProcedural(
            _pointMesh,
            0,
            _flowerMaterial,
            _renderBounds,
            _spawner.FlowerCount,
            _propertyBlock,
            ShadowCastingMode.On,
            true,
            gameObject.layer
        );
    }

    private Mesh CreatePointMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            Vector3.zero,
            Vector3.right * MeshVertexOffset,
            Vector3.forward * MeshVertexOffset
        };

        int[] triangles = new int[] { 0, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * MeshVertexOffset);
        mesh.name = "PointMesh";

        return mesh;
    }
}
