using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Butterfly : MonoBehaviour
{
    private const float FadeDuration = 1f;
    private const float ArrivedThreshold = 1f;
    private const float DirectionEpsilon = 0.001f;
    private const float RotationSmoothTime = 2f;
    private const float WobbleFrequencyBase = 3f;
    private const float WobbleFrequencyRange = 0.5f;
    private const float WobbleAmplitude = 0.5f;
    private const float BobFrequency = 2f;
    private const float BobAmplitude = 0.3f;

    private readonly int _mainTexPropertyId = Shader.PropertyToID("_MainTex");
    private readonly int _alphaPropertyId = Shader.PropertyToID("_Alpha");
    private readonly int _scalePropertyId = Shader.PropertyToID("_Scale");

    [SerializeField] private MeshRenderer _meshRenderer;

    private MeshFilter _meshFilter;
    private float _speed;
    private float _lifetime;
    private float _elapsed;
    private Vector3 _target;
    private Bounds _bounds;
    private float _minimumHeight;
    private float _maximumHeight;
    private Vector3 _currentDirection;
    private MaterialPropertyBlock _propertyBlock;

    public event Action<Butterfly> Died;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        _meshRenderer.GetPropertyBlock(_propertyBlock);
    }

    public void Initialize(Sprite sprite, Bounds bounds, float minimumHeight, float maximumHeight, float speed, float lifetime, float scale)
    {
        _meshFilter.mesh = ButterflyMeshBuilder.BuildMesh(sprite);

        _propertyBlock.SetTexture(_mainTexPropertyId, sprite.texture);

        _bounds = bounds;
        _minimumHeight = minimumHeight;
        _maximumHeight = maximumHeight;
        _speed = speed;
        _lifetime = lifetime;
        _elapsed = 0;
        _currentDirection = Vector3.forward;

        transform.rotation = Quaternion.LookRotation(_currentDirection, Vector3.up);

        _propertyBlock.SetFloat(_scalePropertyId, scale);
        _propertyBlock.SetFloat(_alphaPropertyId, 0);
        _meshRenderer.SetPropertyBlock(_propertyBlock);

        ChooseNewTarget();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= _lifetime)
        {
            Died?.Invoke(this);
            return;
        }

        UpdateAlpha();
        MoveTowardsTarget();
    }

    private void UpdateAlpha()
    {
        float alpha = 1;

        if (_elapsed < FadeDuration)
        {
            alpha = _elapsed / FadeDuration;
        }
        else if (_elapsed > _lifetime - FadeDuration)
        {
            alpha = (_lifetime - _elapsed) / FadeDuration;
        }

        _propertyBlock.SetFloat(_alphaPropertyId, alpha);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void MoveTowardsTarget()
    {
        Vector3 toTarget = _target - transform.position;
        float distance = toTarget.magnitude;

        if (distance < ArrivedThreshold)
        {
            ChooseNewTarget();
            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 moveStep = direction * _speed * Time.deltaTime;

        Vector3 lateralDirection = Vector3.Cross(direction, Vector3.up).normalized;

        if (lateralDirection == Vector3.zero)
            lateralDirection = Vector3.right;

        float wobbleFrequency = WobbleFrequencyBase + UnityEngine.Random.Range(-WobbleFrequencyRange, WobbleFrequencyRange);
        float wobble = Mathf.Sin(_elapsed * wobbleFrequency) * WobbleAmplitude * Time.deltaTime;
        moveStep += lateralDirection * wobble;

        float bob = Mathf.Sin(_elapsed * BobFrequency) * BobAmplitude * Time.deltaTime;
        moveStep += Vector3.up * bob;

        transform.position += moveStep;

        _currentDirection = Vector3.Slerp(_currentDirection, direction, Time.deltaTime * RotationSmoothTime);

        if (_currentDirection.sqrMagnitude > DirectionEpsilon)
            transform.rotation = Quaternion.LookRotation(_currentDirection, Vector3.up);
    }

    private void ChooseNewTarget()
    {
        float x = UnityEngine.Random.Range(_bounds.min.x, _bounds.max.x);
        float z = UnityEngine.Random.Range(_bounds.min.z, _bounds.max.z);
        float y = UnityEngine.Random.Range(_minimumHeight, _maximumHeight);
        _target = new Vector3(x, y, z);

        Vector3 direction = (_target - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            _currentDirection = direction;

            if (_currentDirection.sqrMagnitude > DirectionEpsilon)
                transform.rotation = Quaternion.LookRotation(_currentDirection, Vector3.up);
        }
    }
}
