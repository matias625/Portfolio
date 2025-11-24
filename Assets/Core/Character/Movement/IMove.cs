using UnityEngine;
using UnityEngine.Events;

public interface IMove
{
    // - EVENTS -
    public event UnityAction OnMoveStart;
    public event UnityAction OnMoveEnd;

    public void Event_OnGroundChange(bool isGrounded);

    // - GET -
    public bool IsActive { get; }
    public bool IsRotateActive { get; }

    // - PUBLIC -
    public void CreateMeter(RectTransform container);
    public void CreateGroup(Panel_Stats panelStats);

    public void SetActive(bool isActive);
    public void SetRotateActive(bool isActive);

    public void Move(Vector3 direction);
    public void Rotate(Vector3 direction);

    public void Warp(Vector3 newPosition);
}