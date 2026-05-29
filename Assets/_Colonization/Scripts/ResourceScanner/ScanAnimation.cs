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

    public void Run()
    {
        Reset();

        DoScale(_scaleEntity);
        DoFadeColor(_rendererCircle1);
        DoFadeColor(_rendererCircle2);
        DoFadeColor(_rendererCircle3);
    }

    public void Stop() =>    
        Reset();    

    private void Reset()
    {
        ResetScale(_scaleEntity);
        ResetMeshRenderer();        
    }

    private void ResetMeshRenderer()
    {
        ResetMaterial(_rendererCircle1);
        ResetMaterial(_rendererCircle2);
        ResetMaterial(_rendererCircle3);
    }

    private void ResetMaterial(MeshRenderer renderer) =>
        renderer.material.DOKill();

    private void ResetScale(Transform entity) =>
        entity.DOKill();

    private void DoFadeColor(MeshRenderer renderer)
    {
        ResetColor(renderer);
        renderer.material.DOFade(_transparency, _duration).SetDelay(_delayTime).SetEase(_ease).SetLoops(_repeatCount);
    }

    private void DoScale(Transform entity)
    {
        _scaleEntity.localScale = Vector3.one * _scaleMultiplier;
        entity.DOScale(_scaleVector, _duration).SetDelay(_delayTime).SetEase(_ease).SetLoops(_repeatCount); 
    }
 
    private void ResetColor(MeshRenderer renderer) =>    
        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, _defaultTransparency);
}