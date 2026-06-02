using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Butterfly : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private float _speed;
    private float _lifetime;
    private float _elapsed;
    private Vector3 _target;
    private Bounds _bounds;
    private float _minHeight;
    private float _maxHeight;

    private MaterialPropertyBlock _props;
    private static readonly int AlphaProp = Shader.PropertyToID("_Alpha");
    private static readonly int FlapOffsetProp = Shader.PropertyToID("_FlapOffset");

    public event Action<Butterfly> OnDeath;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _props = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        _spriteRenderer.GetPropertyBlock(_props);
    }

    public void Initialize(Sprite sprite, Bounds bounds, float minHeight, float maxHeight, float speed, float lifetime)
    {
        _spriteRenderer.sprite = sprite;
        _bounds = bounds;
        _minHeight = minHeight;
        _maxHeight = maxHeight;
        _speed = speed;
        _lifetime = lifetime;
        _elapsed = 0;

        float flapOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2);

        _props.SetFloat(FlapOffsetProp, flapOffset);
        _props.SetFloat(AlphaProp, 0);
        _spriteRenderer.SetPropertyBlock(_props);

        ChooseNewTarget();
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= _lifetime)
        {
            OnDeath?.Invoke(this);
            return;
        }

        float alpha = 1;

        if (_elapsed < 1f)
            alpha = _elapsed / 1f;
        else if (_elapsed > _lifetime - 1f)
            alpha = (_lifetime - _elapsed) / 1f;

        _props.SetFloat(AlphaProp, alpha);
        _spriteRenderer.SetPropertyBlock(_props);

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 toTarget = _target - transform.position;
        float distance = toTarget.magnitude;

        if (distance < 1f)
        {
            ChooseNewTarget();
            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 moveStep = direction * _speed * Time.deltaTime;

        Vector3 lateralDir = Vector3.Cross(direction, Vector3.up).normalized;

        if (lateralDir == Vector3.zero)
            lateralDir = Vector3.right;

        float wobbleFreq = 3f + UnityEngine.Random.Range(-0.5f, 0.5f);
        float wobbleAmp = 0.5f;
        float wobble = Mathf.Sin(_elapsed * wobbleFreq) * wobbleAmp * Time.deltaTime;
        moveStep += lateralDir * wobble;

        float bobFreq = 2f;
        float bobAmp = 0.3f;
        float bob = Mathf.Sin(_elapsed * bobFreq) * bobAmp * Time.deltaTime;
        moveStep += Vector3.up * bob;

        transform.position += moveStep;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
    }

    private void ChooseNewTarget()
    {
        float x = UnityEngine.Random.Range(_bounds.min.x, _bounds.max.x);
        float z = UnityEngine.Random.Range(_bounds.min.z, _bounds.max.z);
        float y = UnityEngine.Random.Range(_minHeight, _maxHeight);
        _target = new Vector3(x, y, z);

        Vector3 dir = (_target - transform.position).normalized;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
