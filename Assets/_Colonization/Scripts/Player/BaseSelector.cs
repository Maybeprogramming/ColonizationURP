using System;
using UnityEngine;

public class BaseSelector
{
    private readonly LayerMask _baseLayer;
    private Base _selected;

    public Base Selected => _selected;
    public event Action<Base> SelectionChanged;

    public BaseSelector(LayerMask baseLayer)
    {
        _baseLayer = baseLayer;
    }

    public bool TrySelect(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _baseLayer) == false)
            return false;

        Base clicked = hit.collider.GetComponentInParent<Base>();

        if (clicked == null)
            return false;

        if (clicked == _selected)
            return true;

        _selected = clicked;
        SelectionChanged?.Invoke(clicked);
        return true;
    }

    public void Deselect()
    {
        if (_selected == null)
            return;

        _selected = null;
        SelectionChanged?.Invoke(null);
    }
}
