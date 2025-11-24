using UnityEngine;
using UnityEngine.Events;

public class Dash_Basic : MonoBehaviour, IDash
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
    [SerializeField] private bool _useAim = false;
    [SerializeField] private bool _canJump = false;

    [Header("- Speed -")]
    [SerializeField] float _speed = 10f;
    [SerializeField] float _distance = 5f;

    [Header("- Ground Check -")]
    [SerializeField] private bool _requireOnGround = true;
    private bool _isGrounded = true;

    [Header("- Not Modify -")]
    [SerializeField] private Vector3 _inputDir = Vector3.zero;
    [SerializeField] private bool _isAiming = false;
    [SerializeField] private bool _isMoving = false;

    private float _curDistance = 0f;

    [Header("- Animation -")]
    private const string ANIM_DASH_PREPARATION = "dashPrep";
    private const string ANIM_DASH_CANCEL = "dashCancel";
    private const string ANIM_DASH_USE = "dash";
    private const string ANIM_DASHX = "dashX";
    private const string ANIM_DASHZ = "dashZ";


    // - GET -
    public bool IsAiming => _isAiming;
    public bool IsMoving => _isMoving;
    public bool CanJump => _canJump ? true : !_isMoving;
    Vector3 BodyPosition => _myRigidbody.position;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Dash Basic");
        // Use Aim
        group.CreateToggle("useAim").Configure("Use Aim", _useAim, data => { _useAim = data; });
        // Can Jump 
        group.CreateToggle("canJump").Configure("Can Jump while Dash", _canJump, data => { _canJump = data; });
        // Speed
        group.CreateSlider("moveSpeed").Configure("Move Speed", _speed, 2, 48, data => { _speed = data; });
        // Distance
        group.CreateSlider("distance").Configure("Distance", _distance, 2, 24, data => { _distance = data; });
        // Require On Ground
        group.CreateToggle("reqGround").Configure("Requiere On Ground", _requireOnGround, data => { _requireOnGround = data; });
    }
    public void SetDirection(Vector3 direction)
    {
        _inputDir = direction;
    }
    public void Use(bool pressed, Vector3 direction)
    {
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

        if (_useAim)
        {
            if (pressed)
            {
                _isAiming = true;
                _myAnimator.SetTrigger(ANIM_DASH_PREPARATION);
                OnStart?.Invoke();
                SetDirection(direction);
                return;
            }

            if (!_isAiming) return;
            Debug.Log("Pressed False");
            _myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
            HandleDash();
            _isAiming = false;
            return;
        }

        if (!pressed) return;

        SetDirection(direction);
        HandleDash();
    }
    public void Cancel()
    {
        _isAiming = false;
        _isMoving = false;

        _myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
        _myAnimator.SetTrigger(ANIM_DASH_CANCEL);

        OnEnd?.Invoke();
    }


    // - UNITY -
    private void Update()
    {
        if (Time.timeScale <= 0) return;
        if (!_isAiming) return;
        HandleRotation(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (!_isMoving) return;

        float deltaTime = Time.fixedDeltaTime;
        _curDistance -= deltaTime * _speed;

        _myRigidbody.MovePosition(BodyPosition +
            ((_inputDir * _speed) * deltaTime));

        if (_curDistance > 0) return;

        _curDistance = 0;
        _isMoving = false;
        OnEnd?.Invoke();
    }


    // - PRIVATE -
    private void HandleRotation(float deltaTime)
    {
        Quaternion targetRot = Quaternion.LookRotation(_inputDir, Vector3.up);

        _myRigidbody.MoveRotation(targetRot);
    }
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