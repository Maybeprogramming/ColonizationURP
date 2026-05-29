using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(GrassPainter))]
public class GrassRenderer : MonoBehaviour
{
    [SerializeField] private Mesh _bladeMesh;
    [SerializeField] private Material _grassMaterial;
    [SerializeField] private float _maxViewDistance = 50;

    private GrassPainter _painter;
    private MaterialPropertyBlock _props;
    private Bounds _renderBounds;

    private void Awake()
    {
        _painter = GetComponent<GrassPainter>();
        _props = new MaterialPropertyBlock();

        if (_bladeMesh == null)
            _bladeMesh = CreateTriangleMesh();

        _renderBounds = new Bounds(transform.position, Vector3.one * _maxViewDistance * 2);
    }

    private void Update()
    {
        if (_painter.PositionsBuffer == null || _grassMaterial == null)
            return;

        _props.SetBuffer("_Positions", _painter.PositionsBuffer);

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