using UnityEngine;
using UnityEngine.Events;

public interface IDash
{
    // - EVENT -
    public event UnityAction OnStart;
    public event UnityAction OnEnd;

    public void Event_OnGroundChange(bool isGrounded);


    // - GET -
    public bool IsAiming { get; }
    public bool IsMoving { get; }
    public bool CanJump { get; }
 


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats);
    public void Use(bool pressed, Vector3 direction);
    public void Cancel();
    public void SetDirection(Vector3 direction);
}