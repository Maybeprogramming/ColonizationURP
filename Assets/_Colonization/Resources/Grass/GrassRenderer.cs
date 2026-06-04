using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(GrassPainter))]
public class GrassRenderer : MonoBehaviour
{
    private const float MeshVertexOffset = 0.01f;
    private const float BoundsMultiplier = 2f;

    private readonly int _positionsPropertyId = Shader.PropertyToID("_Positions");

    [SerializeField] private Mesh _bladeMesh;
    [SerializeField] private Material _grassMaterial;
    [SerializeField] private float _maxViewDistance = 50;

    private GrassPainter _painter;
    private MaterialPropertyBlock _propertyBlock;
    private Bounds _renderBounds;

    private void Awake()
    {
        _painter = GetComponent<GrassPainter>();
        _propertyBlock = new MaterialPropertyBlock();

        if (_bladeMesh == null)
            _bladeMesh = CreateTriangleMesh();

        _renderBounds = new Bounds(transform.position, Vector3.one * _maxViewDistance * BoundsMultiplier);
    }

    private void Update()
    {
        if (_painter.PositionsBuffer == null || _grassMaterial == null)
            return;

        _propertyBlock.SetBuffer(_positionsPropertyId, _painter.PositionsBuffer);

        Graphics.DrawMeshInstancedProcedural(
            _bladeMesh,
            0,
            _grassMaterial,
            _renderBounds,
            _painter.BladeCount,
            _propertyBlock,
            ShadowCastingMode.On,
            true,
            gameObject.layer
        );
    }

    private Mesh CreateTriangleMesh()
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

        return mesh;
    }
}
