using UnityEngine;
using UnityEngine.Events;

public class DetectGround_RayCast : MonoBehaviour, IDetectGround
{
    // - EVENTS -
    public event UnityAction<bool> OnGroundChange;

    // - VARIABLES -
    [Header("- Ground Check -")]
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, 0.15f, 0);
    bool _isGrounded = false;

    // - GET -
    public bool IsGrounded => _isGrounded;
    public bool HasGround(Vector3 position)
    {
        return Tool_FloorDetector.HasSomething_Ray(position + _groundCheckOffset,
            Vector3.down, _groundCheckDistance, ~0);
    }

    // - UNITY -
    void FixedUpdate()
    {
        bool grounded = Tool_FloorDetector.HasSomething_Ray(transform.position + _groundCheckOffset,
            Vector3.down, _groundCheckDistance, ~0);

        if (_isGrounded == grounded) return;

        OnGroundChange?.Invoke(grounded);
        _isGrounded = grounded;
    }
}