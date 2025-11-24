using UnityEngine;
using UnityEngine.Events;

public class Jump_Aim : MonoBehaviour, IJump
{
    // - EVENTS -
    public event UnityAction OnStart;
    public event UnityAction OnEnd;

    public void Event_OnGroundChange(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }

    // - VARIABLES -
    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _myAnimator;

    [Header("- Options -")]
    [SerializeField] bool _canMoveOnAir = true;

    [Header("- Jump -")]
    [SerializeField] float _jumpForce = 6f;

    [Header("- Extra Jumps -")]
    [SerializeField] private int _extraJumps = 0;
    [Tooltip("Force reduced by jumps made")]
    [SerializeField] private float _forceReduced = 2f;
    private int _curJumps = 0;

    [Header("- Ground Check -")]
    [SerializeField] private bool _requireOnGround = true;
    private bool _isGrounded = true;

    private const string ANIM_JUMP = "jump";
    private const string ANIM_ON_GROUND = "onGround";


    // - GET -
    public bool CanMoveOnAir => _canMoveOnAir;
    Vector3 BodyPosition => _myRigidbody.transform.position;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Jump Aim");
        // Require Ground
        group.CreateToggle("reqGround").Configure("Requiere On Ground", _requireOnGround, data => { _requireOnGround = data; });
        // Jump Force
        group.CreateSlider("jumpForce").Configure("Jump Force", _jumpForce, 1, 20, data => { _jumpForce = data; });
        // Can Move on Air
        group.CreateToggle("canMoveOnAir").Configure("Can Move On Air", _canMoveOnAir, data => { _canMoveOnAir = data; });
        // Extra Jumps
        group.CreateSlider("jumpExtra").Configure("Extra Jumps", _extraJumps, 0, 10, data => { _extraJumps = Mathf.RoundToInt(data); }, true);
        group.CreateSlider("jumpForceRed").Configure("Jump Force Reduced", _forceReduced, 0, 10, data => { _forceReduced = data; });
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
        float totalForce = _jumpForce - (_forceReduced * _curJumps);

        _myRigidbody.AddForce(direction + (Vector3.up * totalForce), ForceMode.Impulse);
        _myAnimator.SetTrigger(ANIM_JUMP);

        if (_curJumps == 0)
            OnStart?.Invoke();

        _curJumps++;
    }


    // - UNITY -
    void Update()
    {
        if (_curJumps > 0 && _isGrounded)
        {
            _curJumps = 0;
            OnEnd.Invoke();
        }            

        _myAnimator.SetBool(ANIM_ON_GROUND, _isGrounded);
    }  
}