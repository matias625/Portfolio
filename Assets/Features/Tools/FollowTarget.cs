using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = false;
    [SerializeField] private bool _followZ = true;



    void Update()
    {
        Vector3 newPos = transform.position;
        Vector3 targetPos = _target.position;
        if (_followX)
            newPos.x = targetPos.x;
        if (_followY)
            newPos.y = targetPos.y;
        if (_followZ)
            newPos.z = targetPos.z;

        transform.position = newPos;
    }
}
