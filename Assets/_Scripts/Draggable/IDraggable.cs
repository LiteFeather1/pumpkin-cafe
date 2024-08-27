using UnityEngine;

public interface IDraggable : System.IComparable<IDraggable>
{
    public int Order { get; }
    public DraggingMethod Method { get; }
    public System.Action OnGrabbed { get; set; }
    public System.Action OnReleased { get; set; }
    public System.Action OnHold { get; set; }
    public System.Action OnForceReleased { get; set; }
    public Rigidbody2D RB { get; }
    public Collider2D Collider { get; }
    public bool IsHold { get; set; }
    public bool IsDragging { get; set; }

    public virtual void StartDragging()
    {
        IsDragging = true;
        if (RB.bodyType != RigidbodyType2D.Static)
            RB.velocity = Vector2.zero;
        RB.gravityScale = 0f;
        RB.rotation = 0f;
        RB.freezeRotation = true;
        Collider.isTrigger = true;
        OnGrabbed?.Invoke();
    }

    public void StopDragging()
    {
        IsDragging = false;
        RB.gravityScale = 1f;
        RB.freezeRotation = false;
        Collider.isTrigger = false;
        OnReleased?.Invoke();
    }

    public void Hold()
    {
        IsHold = true;
        OnHold?.Invoke();
    }

    public void ForceRelease() => OnForceReleased?.Invoke();

    public enum DraggingMethod
    {
        Teleport,
        Move,
    }
}
