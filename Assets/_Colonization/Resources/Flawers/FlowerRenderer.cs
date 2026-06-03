using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(FlowerSpawner))]
public class FlowerRenderer : MonoBehaviour
{
    [SerializeField] private Material _flowerMaterial;
    [SerializeField] private float _maxViewDistance = 50;

    private FlowerSpawner _spawner;
    private MaterialPropertyBlock _props;
    private Bounds _renderBounds;

    private void Awake()
    {
        _spawner = GetComponent<FlowerSpawner>();
        _props = new MaterialPropertyBlock();

        _renderBounds = new Bounds(transform.position, Vector3.one * _maxViewDistance * 2);
    }

    private void Update()
    {
        if (_spawner.PositionsBuffer == null || _flowerMaterial == null)
            return;

        _props.SetBuffer("_Positions", _spawner.PositionsBuffer);

        Graphics.DrawMeshInstancedProcedural(
            GetPointMesh(),
            0,
            _flowerMaterial,
            _renderBounds,
            _spawner.FlowerCount,
            _props,
            ShadowCastingMode.On,
            true,
            gameObject.layer
        );
    }

    private static Mesh _bladeMesh;

    private static Mesh GetPointMesh()
    {
        if (_bladeMesh != null)
            return _bladeMesh;

        _bladeMesh = new Mesh();
        _bladeMesh.vertices = new Vector3[]
        {
            Vector3.zero,
            Vector3.right * 0.01f,
            Vector3.forward * 0.01f
        };
        _bladeMesh.triangles = new int[] { 0, 1, 2 };
        _bladeMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 0.01f);
        _bladeMesh.name = "PointMesh";
        return _bladeMesh;
    }
}
