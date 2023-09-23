using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMasks;
    [SerializeField] private LayerMask _draggingLayer;

    [Header("Dragging")]
    [SerializeField] private Vector2 _speedRange;
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _distanceToMaxSpeed;
    private int _draggablePrevLayer;
    private Vector2 _previousMousePos;
    private Vector2 _releaseVelocity;
    private IDraggable _draggable;

    public IDraggable Draggable => _draggable;

    private void OnEnable()
    {
        InputAction leftClick = GameManager.InputManager.PlayerInputs.LeftClick;
        leftClick.performed += OnClick;
        leftClick.canceled += OnReleaseClick;

        GameManager.InputManager.PlayerInputs.RightClick.performed += DeleteIngredient;
    }

    private void FixedUpdate() => DragMovement();

    private void OnDisable()
    {
        InputAction leftClick = GameManager.InputManager.PlayerInputs.LeftClick;
        leftClick.performed -= OnClick;
        leftClick.canceled -= OnReleaseClick;

        GameManager.InputManager.PlayerInputs.RightClick.performed -= DeleteIngredient;
    }

    private void DragMovement()
    {
        if (_draggable == null)
            return;

        Vector2 mousePos = GameManager.MousePosition();
        _draggable.RB.position = mousePos;
        float distance = Vector2.Distance(_previousMousePos, mousePos);
        if (distance < 0.1f)
        {
            _releaseVelocity = Vector2.zero;
            return;
        }

        float t = _speedCurve.Evaluate(distance / _distanceToMaxSpeed);
        float speed = Mathf.Lerp(_speedRange.x, _speedRange.y, t);
        Vector2 direction = (mousePos - _previousMousePos).normalized;
        _releaseVelocity = speed * direction;
        _previousMousePos = mousePos;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Vector2 mousePoint = GameManager.MousePosition();
        var collider = Physics2D.OverlapPoint(mousePoint, _layerMasks);
        if (collider == null)
            return;

        if (collider.TryGetComponent(out IDraggable draggable))
            StartDrag(draggable);
        else if (collider.TryGetComponent(out IGiveDraggable giveDraggable))
            StartDrag(giveDraggable.GetDraggable(mousePoint));
    }

    public void StartDrag(IDraggable draggable)
    {
        if (draggable.IsHold) 
            return;

        draggable.OnForceReleased += OnForceRelease;
        draggable.StartDragging();
        _draggable = draggable;
        _draggablePrevLayer = draggable.RB.gameObject.layer;
        draggable.RB.gameObject.layer = (int)Mathf.Log(_draggingLayer.value, 2f);
    }

    private void OnReleaseClick(InputAction.CallbackContext ctx)
    {
        if (_draggable == null)
            return;

        _draggable.RB.velocity = _releaseVelocity;
        OnForceRelease();
    }

    private void OnForceRelease()
    {
        _draggable.OnForceReleased -= OnForceRelease;
        _draggable.RB.gameObject.layer = _draggablePrevLayer;
        _draggable.StopDragging();
        _draggable = null;
    }

    private void DeleteIngredient(InputAction.CallbackContext ctx)
    {
        Vector2 mousePoint = GameManager.MousePosition();
        var collider = Physics2D.OverlapPoint(mousePoint, _layerMasks);
        if (collider == null)
            return;

        if (collider.TryGetComponent(out IDestroyable destroyable))
            destroyable.Destroy();
    }
}