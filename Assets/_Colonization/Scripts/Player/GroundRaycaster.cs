using UnityEngine;

public class GroundRaycaster
{
    private readonly LayerMask _groundLayer;
    private readonly Bounds _bounds;

    public GroundRaycaster(LayerMask groundLayer, Bounds bounds)
    {
        _groundLayer = groundLayer;
        _bounds = bounds;
    }

    public bool TryGetGroundPoint(Ray ray, out Vector3 point)
    {
        point = default;

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _groundLayer) == false)
            return false;

        if (_bounds.Contains(hit.point) == false)
            return false;

        point = hit.point;
        return true;
    }
}
