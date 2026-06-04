using UnityEngine;
using UnityEngine.InputSystem;

public class FlagPlacer : MonoBehaviour
{
    private const float SelectionRectHeight = 0.05f;
    private const float SelectionRectYOffset = 0.51f;
    private const float FallbackBoundsSize = 2f;
    private const float PreviewScale = 0.3f;

    [SerializeField] private Bounds _mapBounds;
    [SerializeField] private GameObject _flagPrefab;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _baseLayer;
    [SerializeField] private GameObject _selectionRect;
    [SerializeField] private Color _selectionColor = Color.red;
    [SerializeField, Range(0f, 3f)] private float _selectionRectScale = 1.2f;

    private Base _selectedBase;
    private GameObject _flagVisual;
    private Camera _camera;
    private Material _selectionMaterial;
    private bool _isFollowingMouse;

    private void Awake()
    {
        _camera = Camera.main;

        if (_selectionRect == null)
            CreateSelectionRect();

        if (_mapBounds.size == Vector3.zero)
        {
            GameObject ground = GameObject.Find("Ground");

            if (ground != null)
            {
                Renderer groundRenderer = ground.GetComponent<Renderer>();

                if (groundRenderer != null)
                    _mapBounds = groundRenderer.bounds;

                _groundLayer = 1 << ground.layer;
            }
        }

        if (_baseLayer == 0)
        {
            Base firstBase = Object.FindFirstObjectByType<Base>();

            if (firstBase != null)
                _baseLayer = 1 << firstBase.gameObject.layer;
        }
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && _selectedBase != null)
        {
            _selectedBase.HasConstractNewBase = false;
            _selectedBase.CancelConstructTasks();
            RemoveFlag();
            return;
        }

        if (_isFollowingMouse)
            UpdatePreviewPosition();

        if (Mouse.current.leftButton.wasPressedThisFrame == false)
            return;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (TrySelectBase(ray))
            return;

        if (_selectedBase != null && TryPlaceFlag(ray))
            return;
    }

    private void OnDestroy()
    {
        if (_selectedBase != null)
            _selectedBase.NewBaseBuilt -= RemoveFlag;

        if (_flagVisual != null)
            Destroy(_flagVisual);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_mapBounds.center, _mapBounds.size);
    }

    public void RemoveFlag()
    {
        if (_selectedBase != null)
            _selectedBase.NewBaseBuilt -= RemoveFlag;

        _selectionRect.gameObject.SetActive(false);
        ClearFlagVisual();
        _isFollowingMouse = false;
        _selectedBase = null;
    }

    private void CreateSelectionRect()
    {
        _selectionRect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _selectionRect.name = "SelectionRect";
        _selectionRect.transform.SetParent(transform, false);

        MeshRenderer meshRenderer = _selectionRect.GetComponent<MeshRenderer>();
        _selectionMaterial = new Material(Shader.Find("Unlit/Color"));
        _selectionMaterial.color = _selectionColor;
        meshRenderer.material = _selectionMaterial;

        _selectionRect.SetActive(false);
    }

    private void ClearFlagVisual()
    {
        if (_flagVisual != null)
            _flagVisual.SetActive(false);
    }

    private void EnsureFlagVisual()
    {
        if (_flagVisual != null)
            return;

        if (_flagPrefab != null)
        {
            _flagVisual = Instantiate(_flagPrefab);
        }
        else
        {
            _flagVisual = new GameObject("BuildingPreview");
            _flagVisual.transform.localScale = Vector3.one * PreviewScale;

            MeshFilter meshFilter = _flagVisual.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = Resources.Load<Mesh>("BuildingBarn/MeshBuildingBarn");

            MeshRenderer meshRenderer = _flagVisual.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("BaseModel/BasePreview");
        }

        _flagVisual.SetActive(false);
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _groundLayer) == false)
            return;

        Vector3 position = hit.point;

        if (_mapBounds.Contains(position) == false)
            return;

        _flagVisual.transform.position = position;
        _flagVisual.SetActive(true);
    }

    private void ShowFlag(Vector3 position)
    {
        EnsureFlagVisual();
        _flagVisual.transform.position = position;
        _flagVisual.SetActive(true);
        _isFollowingMouse = false;
    }

    private void ShowSelectionRect()
    {
        Renderer baseRenderer = _selectedBase.GetComponentInChildren<Renderer>();
        Bounds bounds = baseRenderer != null ? baseRenderer.bounds : new Bounds(_selectedBase.transform.position, Vector3.one * FallbackBoundsSize);
        Vector3 size = bounds.size * _selectionRectScale;
        size.y = SelectionRectHeight;

        _selectionRect.transform.localScale = size;
        _selectionRect.transform.position = new Vector3(bounds.center.x, SelectionRectYOffset, bounds.center.z);
        _selectionMaterial.color = _selectionColor;
        _selectionRect.SetActive(true);
    }

    private bool TryPlaceFlag(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _groundLayer) == false)
            return false;

        Vector3 position = hit.point;

        if (_mapBounds.Contains(position) == false)
            return false;

        _selectedBase.FlagPosition = position;
        _selectedBase.HasConstractNewBase = true;

        ShowFlag(position);

        return true;
    }

    private bool TrySelectBase(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _baseLayer) == false)
            return false;

        Base clickedBase = hit.collider.GetComponentInParent<Base>();

        if (clickedBase == null)
            return false;

        if (clickedBase == _selectedBase)
        {
            if (_isFollowingMouse == false)
            {
                EnsureFlagVisual();
                _isFollowingMouse = true;
                UpdatePreviewPosition();
            }

            return true;
        }

        if (_selectedBase != null)
        {
            _selectedBase.NewBaseBuilt -= RemoveFlag;
            _selectedBase.HasConstractNewBase = false;
            _selectedBase.CancelConstructTasks();
            ClearFlagVisual();
            _isFollowingMouse = false;
        }

        _selectedBase = clickedBase;
        _selectedBase.NewBaseBuilt += RemoveFlag;
        ShowSelectionRect();
        EnsureFlagVisual();
        _isFollowingMouse = true;
        UpdatePreviewPosition();
        return true;
    }
}
