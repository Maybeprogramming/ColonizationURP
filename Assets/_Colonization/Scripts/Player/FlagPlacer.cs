using UnityEngine;
using UnityEngine.InputSystem;

public class FlagPlacer : MonoBehaviour
{
    [SerializeField] private GameObject _flagPrefab;
    [SerializeField] private Color _selectionColor = Color.red;
    [SerializeField, Range(0f, 3f)] private float _selectionRectScale = 1.2f;

    private Camera _camera;
    private BaseSelector _selector;
    private GroundRaycaster _groundRaycaster;
    private FlagVisualProvider _flagVisual;
    private SelectionRectRenderer _selectionRect;
    private bool _isFollowingMouse;
    private Base _builtHandlerTarget;

    private void Awake()
    {
        _camera = Camera.main;
        _selector = new BaseSelector(GameContext.BaseLayer);
        _groundRaycaster = new GroundRaycaster(GameContext.GroundLayer, GameContext.MapBounds);
        _flagVisual = new FlagVisualProvider(_flagPrefab);
        _selectionRect = new SelectionRectRenderer(transform, _selectionColor);
        _selectionRect.Initialize();
        _selector.SelectionChanged += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        if (_selector != null)
            _selector.SelectionChanged -= OnSelectionChanged;

        UnsubscribeFromBaseBuilt();

        _flagVisual?.Destroy();
        _selectionRect?.Destroy();
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            CancelSelection();
            return;
        }

        if (_isFollowingMouse && _groundRaycaster.TryGetGroundPoint(GetMouseRay(mouse), out Vector3 hoverPoint))
            _flagVisual.Show(hoverPoint);

        if (mouse.leftButton.wasPressedThisFrame == false)
            return;

        Ray ray = GetMouseRay(mouse);

        if (_selector.TrySelect(ray))
        {
            StartFollowing();
            return;
        }

        if (_selector.Selected != null)
            TryPlaceFlag(ray);
    }

    private Ray GetMouseRay(Mouse mouse) =>
        _camera.ScreenPointToRay(mouse.position.ReadValue());

    private void OnSelectionChanged(Base newSelection)
    {
        Base previous = _builtHandlerTarget;
        UnsubscribeFromBaseBuilt();

        if (previous != null)
        {
            previous.CancelExpansion();
            previous.CancelConstructTasks();
        }

        _flagVisual.Hide();
        _isFollowingMouse = false;

        if (newSelection == null)
        {
            _selectionRect.Hide();
            return;
        }

        newSelection.NewBaseBuilt += RemoveFlag;
        _builtHandlerTarget = newSelection;
        _selectionRect.Show(newSelection, _selectionRectScale);
    }

    private void TryPlaceFlag(Ray ray)
    {
        if (_groundRaycaster.TryGetGroundPoint(ray, out Vector3 point) == false)
            return;

        _selector.Selected.AssignExpansionFlag(point);
        _flagVisual.Show(point);
        _isFollowingMouse = false;
    }

    private void StartFollowing()
    {
        if (_isFollowingMouse)
            return;

        _isFollowingMouse = true;

        if (_groundRaycaster.TryGetGroundPoint(GetMouseRay(Mouse.current), out Vector3 point))
            _flagVisual.Show(point);
    }

    private void CancelSelection()
    {
        if (_selector.Selected == null)
            return;

        _selector.Selected.CancelExpansion();
        _selector.Selected.CancelConstructTasks();
        _selector.Deselect();
        _flagVisual.Hide();
        _selectionRect.Hide();
        _isFollowingMouse = false;
    }

    private void RemoveFlag()
    {
        _selector.Deselect();
        _flagVisual.Hide();
        _selectionRect.Hide();
        _isFollowingMouse = false;
    }

    private void UnsubscribeFromBaseBuilt()
    {
        if (_builtHandlerTarget == null)
            return;

        _builtHandlerTarget.NewBaseBuilt -= RemoveFlag;
        _builtHandlerTarget = null;
    }
}
