using UnityEngine;
using UnityEngine.Events;

public class DetectGround_Collision : MonoBehaviour, IDetectGround
{
    // - EVENTS -
    public event UnityAction<bool> OnGroundChange;


    // - VARIABLES -
    [SerializeField] bool _isGrounded = true;


    // - GET -
    public bool IsGrounded => _isGrounded;
    public bool HasGround(Vector3 position)
    {
        return Tool_FloorDetector.HasSomething_Ray(position + (Vector3.up * .2f),
            Vector3.down, .25f, ~0);
    }

    // - PRIVATE -
    void ChangeGround(bool isGround)
    {
        if (_isGrounded != isGround)
            OnGroundChange?.Invoke(isGround);

        _isGrounded = isGround;
    }


    // - UNITY -
    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                ChangeGround(true);
                return;
            }
        }
        ChangeGround(false);
    }

    void OnCollisionExit(Collision collision)
    {
        ChangeGround(false);
    }
}