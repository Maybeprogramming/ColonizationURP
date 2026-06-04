using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GameContext : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private ResourcesData _resourcesData;
    [SerializeField] private SpawnerResources _spawner;
    [SerializeField] private BotFactory _botFactory;
    [SerializeField] private BaseFactory _baseFactory;

    [Header("Scene")]
    [SerializeField] private Base _startBase;
    [SerializeField] private ResourceCounterView _counterView;

    [Header("Layers & Bounds")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _baseLayer;
    [SerializeField] private Bounds _mapBounds;
    [SerializeField] private GameObject _groundReference;

    public static GameContext Instance { get; private set; }

    public static ResourcesData ResourcesData => Instance != null ? Instance._resourcesData : null;
    public static SpawnerResources Spawner => Instance != null ? Instance._spawner : null;
    public static BotFactory BotFactory => Instance != null ? Instance._botFactory : null;
    public static BaseFactory BaseFactory => Instance != null ? Instance._baseFactory : null;
    public static Base StartBase => Instance != null ? Instance._startBase : null;
    public static LayerMask GroundLayer => Instance != null ? Instance._groundLayer : default;
    public static LayerMask BaseLayer => Instance != null ? Instance._baseLayer : default;
    public static Bounds MapBounds => Instance != null ? Instance._mapBounds : default;

    private BaseEventBinder _eventBinder;
    private ResourceWarhouse _startWarhouse;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"{nameof(GameContext)}: duplicate instance on '{name}', destroying.", this);
            Destroy(gameObject);
            return;
        }

        if (GetComponent<FlagPlacer>() == null)
            gameObject.AddComponent<FlagPlacer>();

        if (_startBase == null)
            return;

        _startWarhouse = _startBase.GetComponent<ResourceWarhouse>();
        _eventBinder = new BaseEventBinder(_resourcesData, _spawner);
        _eventBinder.Bind(_startBase);

        if (_counterView != null && _startWarhouse != null)
            _startWarhouse.Changed += _counterView.CountUpdateHandler;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (_eventBinder != null && _startBase != null)
            _eventBinder.Unbind(_startBase);

        if (_counterView != null && _startWarhouse != null)
            _startWarhouse.Changed -= _counterView.CountUpdateHandler;

        Instance = null;
    }

    [ContextMenu("Auto-Fill From Ground")]
    private void AutoFillFromGround()
    {
        if (_groundReference == null)
        {
            _groundReference = GameObject.Find("Ground");
        }

        if (_groundReference == null)
        {
            Debug.LogWarning($"{nameof(GameContext)}: Ground GameObject not found.", this);
            return;
        }

        Renderer groundRenderer = _groundReference.GetComponent<Renderer>();

        if (groundRenderer == null)
        {
            Debug.LogWarning($"{nameof(GameContext)}: '{_groundReference.name}' has no Renderer.", this);
            return;
        }

        _mapBounds = groundRenderer.bounds;
        _groundLayer = 1 << _groundReference.layer;

        Debug.Log($"{nameof(GameContext)}: bounds={_mapBounds.center}+{_mapBounds.size}, groundLayer={_groundLayer.value}", this);
    }
}
