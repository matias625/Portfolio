using UnityEngine;
using UnityEngine.Events;

public interface IJump
{
    // - EVENT -
    public event UnityAction OnStart;
    public event UnityAction OnEnd;

    public void Event_OnGroundChange(bool isGrounded);

    // - GET -
    public bool CanMoveOnAir { get; }


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats);
    public void Use(Vector3 direction);
}