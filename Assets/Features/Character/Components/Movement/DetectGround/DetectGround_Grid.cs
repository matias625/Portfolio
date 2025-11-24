using UnityEngine;
using UnityEngine.Events;

public class DetectGround_Grid : MonoBehaviour, IDetectGround
{
    // - EVENTS -
    public event UnityAction<bool> OnGroundChange;

    // - GET -
    public bool IsGrounded => GridManager.Instance.HasWorldFloor(transform.position);
    public bool HasGround(Vector3 position)
    {
        return GridManager.Instance.HasWorldFloor(position);
    }
}