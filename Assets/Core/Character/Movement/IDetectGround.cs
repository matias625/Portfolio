using UnityEngine;
using UnityEngine.Events;

public interface IDetectGround
{
    public event UnityAction<bool> OnGroundChange;
    
    public bool IsGrounded { get; }
    public bool HasGround(Vector3 position);
}