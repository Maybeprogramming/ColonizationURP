using DG.Tweening;
using UnityEngine;

public class ScanAnimation : MonoBehaviour
{
    [SerializeField] private Transform _scaleEntity;
    [SerializeField] private Vector3 _scaleVector;
    [SerializeField] private float _duration;
    [SerializeField] private float _delayTime;
    [SerializeField] private Ease _ease;
    [SerializeField] private int _repeatCount;
    [SerializeField] private MeshRenderer _rendererCircle1;
    [SerializeField] private MeshRenderer _rendererCircle2;
    [SerializeField] private MeshRenderer _rendererCircle3;
    [SerializeField] private float _transparency;
    [SerializeField] private float _defaultTransparency;
    [SerializeField] private float _scaleMultiplier;

    private Material _materialCircle1;
    private Material _materialCircle2;
    private Material _materialCircle3;

    public void Run()
    {
        EnsureMaterials();
        Reset();

        DoScale(_scaleEntity);
        DoFadeColor(_materialCircle1);
        DoFadeColor(_materialCircle2);
        DoFadeColor(_materialCircle3);
    }

    public void Stop()
    {
        EnsureMaterials();
        Reset();
    }

    private void EnsureMaterials()
    {
        if (_materialCircle1 == null) _materialCircle1 = _rendererCircle1.material;
        if (_materialCircle2 == null) _materialCircle2 = _rendererCircle2.material;
        if (_materialCircle3 == null) _materialCircle3 = _rendererCircle3.material;
    }

    private void Reset()
    {
        ResetScale(_scaleEntity);
        ResetMaterial(_materialCircle1);
        ResetMaterial(_materialCircle2);
        ResetMaterial(_materialCircle3);
    }

    private void ResetMaterial(Material material) =>
        material.DOKill();

    private void ResetScale(Transform entity) =>
        entity.DOKill();

    private void DoFadeColor(Material material)
    {
        ResetColor(material);
        material.DOFade(_transparency, _duration).SetDelay(_delayTime).SetEase(_ease).SetLoops(_repeatCount);
    }

    private void DoScale(Transform entity)
    {
        _scaleEntity.localScale = Vector3.one * _scaleMultiplier;
        entity.DOScale(_scaleVector, _duration).SetDelay(_delayTime).SetEase(_ease).SetLoops(_repeatCount);
    }

    private void ResetColor(Material material) =>
        material.color = new Color(material.color.r, material.color.g, material.color.b, _defaultTransparency);
}
