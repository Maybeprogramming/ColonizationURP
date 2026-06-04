using UnityEngine;

public class SelectionRectRenderer
{
    private const float Height = 0.05f;
    private const float YOffset = 0.51f;
    private const float FallbackBoundsSize = 2f;

    private readonly Transform _parent;
    private readonly Color _color;
    private GameObject _rect;
    private Material _material;

    public SelectionRectRenderer(Transform parent, Color color)
    {
        _parent = parent;
        _color = color;
    }

    public void Initialize()
    {
        _rect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _rect.name = "SelectionRect";
        _rect.transform.SetParent(_parent, false);

        MeshRenderer meshRenderer = _rect.GetComponent<MeshRenderer>();
        _material = new Material(Shader.Find("Unlit/Color"));
        _material.color = _color;
        meshRenderer.material = _material;

        _rect.SetActive(false);
    }

    public void Show(Base targetBase, float scale)
    {
        Renderer baseRenderer = targetBase.GetComponentInChildren<Renderer>();
        Bounds bounds = baseRenderer != null
            ? baseRenderer.bounds
            : new Bounds(targetBase.transform.position, Vector3.one * FallbackBoundsSize);

        Vector3 size = bounds.size * scale;
        size.y = Height;

        _rect.transform.localScale = size;
        _rect.transform.position = new Vector3(bounds.center.x, YOffset, bounds.center.z);
        _material.color = _color;
        _rect.SetActive(true);
    }

    public void Hide()
    {
        if (_rect != null)
            _rect.SetActive(false);
    }

    public void Destroy()
    {
        if (_rect != null)
            Object.Destroy(_rect);

        if (_material != null)
            Object.Destroy(_material);
    }
}
