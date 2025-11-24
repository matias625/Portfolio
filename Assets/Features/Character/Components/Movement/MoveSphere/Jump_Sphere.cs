using UnityEngine;
using UnityEngine.Events;

public class Jump_Sphere : MonoBehaviour, IJump
{
    // - EVENT -
    public event UnityAction OnStart;
    public event UnityAction OnEnd;

    public void Event_OnGroundChange(bool isGrounded)
    {
        _isGrounded = isGrounded;

        if (_isGrounded && _curJumps > 0)
            _curJumps = 0;
    }


    // - VARIABLES -
    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;

    [Header("- Options -")]
    [SerializeField] bool _canMoveOnAir = true;

    [Header("- Jump -")]
    [SerializeField] float _jumpForce = 6f;

    [Header("- Extra Jumps -")]
    [SerializeField] private int _extraJumps = 0;
    private int _curJumps = 0;

    [Header("- Ground Check -")]
    [SerializeField] private bool _requireOnGround = true;
    private bool _isGrounded = true;


    // - GET -
    public bool CanMoveOnAir => _canMoveOnAir;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Jump Aim");
        // Require Ground
        group.CreateToggle("reqGround").Configure("Requiere On Ground", _requireOnGround, data => { _requireOnGround = data; });
        // Jump Force
        group.CreateSlider("jumpForce").Configure("Jump Force", _jumpForce, 1, 30, data => { _jumpForce = data; });
        // Can Move on Air
        group.CreateToggle("canMoveOnAir").Configure("Can Move On Air", _canMoveOnAir, data => { _canMoveOnAir = data; });
        // Extra Jumps
        group.CreateSlider("jumpExtra").Configure("Extra Jumps", _extraJumps, 0, 10, data => { _extraJumps = Mathf.RoundToInt(data); }, true);        
    }
    public void Use(Vector3 direction)
    {
        if (_requireOnGround && !_isGrounded)
        {
            if (_extraJumps == 0 || _curJumps > _extraJumps)
            {
                return;
            }
        }

        _myRigidbody.AddForce(direction + (Vector3.up * _jumpForce), ForceMode.Impulse);
        // _myAnimator.SetTrigger(ANIM_JUMP);

        if (_curJumps == 0)
            OnStart?.Invoke();
        
        _curJumps++;
    }
}