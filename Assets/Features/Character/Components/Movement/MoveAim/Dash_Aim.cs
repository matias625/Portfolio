using UnityEngine;
using UnityEngine.Events;

public class Dash_Aim : MonoBehaviour, IDash
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
    [SerializeField] private bool _canJump = false;

    [Header("- Speed -")]
    [SerializeField] float _speed = 10f;
    [SerializeField] float _distance = 5f;

    [Header("- Ground Check -")]
    [SerializeField] private bool _requireOnGround = true;
    private bool _isGrounded = true;

    [Header("- Not Modify -")]
    [SerializeField] private Vector3 _inputDir = Vector3.zero;
    [SerializeField] private bool _isMoving = false;

    private float _curDistance = 0f;

    [Header("- Animation -")]
    private const string ANIM_DASH_USE = "dash";
    private const string ANIM_DASHX = "dashX";
    private const string ANIM_DASHZ = "dashZ";


    // - GET -
    public bool IsAiming => false;
    public bool IsMoving => _isMoving;
    public bool CanJump => _canJump ? true : !_isMoving;
    Vector3 BodyPosition => _myRigidbody.transform.position;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Dash Aim");
        // Can Jump 
        group.CreateToggle("canJump").Configure("Can Jump while Dash", _canJump, data => { _canJump = data; });
        // Speed
        group.CreateSlider("moveSpeed").Configure("Move Speed", _speed, 2, 48, data => { _speed = data; });
        // Distance
        group.CreateSlider("distance").Configure("Distance", _distance, 2, 24, data => { _distance = data; });
        // Require On Ground
        group.CreateToggle("reqGround").Configure("Requiere On Ground", _requireOnGround, data => { _requireOnGround = data; });
    }
    public void Use(bool pressed, Vector3 direction)
    {
        if (!pressed) return;
        if (_isMoving)
        {
            Debug.Log("Already using Dash.");
            return;
        }
        if (_requireOnGround && !_isGrounded)
        {
            Debug.Log("Require to be on the ground.");
            return;
        }

        SetDirection(direction);
        HandleDash();
    }
    public void Cancel()
    {
        _isMoving = false;

        OnEnd?.Invoke();
    }
    public void SetDirection(Vector3 direction)
    {
        _inputDir = direction;
    }


    // - UNITY -
    private void FixedUpdate()
    {
        if (!_isMoving) return;

        float deltaTime = Time.fixedDeltaTime;
        _curDistance -= deltaTime * _speed;

        _myRigidbody.MovePosition(_myRigidbody.position +
            ((_inputDir * _speed) * deltaTime));

        if (_curDistance > 0) return;

        _curDistance = 0;
        _isMoving = false;
        OnEnd?.Invoke();
    }


    // - PRIVATE -
    void HandleDash()
    {
        _curDistance = _distance;
        _isMoving = true;

        // Animation
        Vector3 animDir = transform.InverseTransformDirection(_inputDir);
        _myAnimator.SetFloat(ANIM_DASHX, animDir.x);
        _myAnimator.SetFloat(ANIM_DASHZ, animDir.z);
        _myAnimator.SetTrigger(ANIM_DASH_USE);

        OnStart?.Invoke();
    }
}