using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputSystem : MonoBehaviour
{
    private InputSystem_Actions _inputSystem;

    public Vector3 MoveDirection => GetDirection();
    public Vector2 LookDelta => _inputSystem.Player.Look.ReadValue<Vector2>();
    public bool IsMiddleMousePressed => Mouse.current.middleButton.isPressed;
    public Vector2 ScrollDelta => _inputSystem.UI.ScrollWheel.ReadValue<Vector2>();

    private void Awake()
    {
        _inputSystem = new();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.Disable();
    }

    private Vector3 GetDirection()
    {
        return new Vector3(_inputSystem.Player.Move.ReadValue<Vector2>().x,
                            Vector3.zero.y,
                            _inputSystem.Player.Move.ReadValue<Vector2>().y);
    }
}