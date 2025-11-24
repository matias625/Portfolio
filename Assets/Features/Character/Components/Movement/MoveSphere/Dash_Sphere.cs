using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Dash_Sphere : MonoBehaviour, IDash
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

    [Header("- Options -")]
    [SerializeField] private bool _useAim = false;

    [Header("- Speed -")]
    [SerializeField] float _force = 10f;
    [SerializeField] float _duration = 1f;
    float _timerDuration = 0;

    [Header("- Cooldown -")]
    [SerializeField] float _cooldown = 2f;
    private float _timerCooldown = 0f;

    [Header("- Ground Check -")]
    [SerializeField] private bool _requireOnGround = true;
    private bool _isGrounded = false;

    [Header("- Not Modify -")]
    [SerializeField] private Vector3 _inputDir = Vector3.zero;
    [SerializeField] private bool _isAiming = false;
     

    // - GET -
    public bool IsAiming => _isAiming;
    public bool IsMoving => _timerDuration > 0;
    public bool CanJump => true; // _canJump ? true : !_isMoving;
    Vector3 BodyPosition => _myRigidbody.position;


    // - PUBLIC -

    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Dash Basic");
        // Use Aim
        group.CreateToggle("useAim").Configure("Use Aim", _useAim, data => { _useAim = data; });
        // Force
        group.CreateSlider("moveForce").Configure("Move Fore", _force, 2, 48, data => { _force = data; });
        // Duration
        group.CreateSlider("duration").Configure("Duration", _duration, .1f, 4f, data => { _duration = data; });
        // Cooldown
        group.CreateSlider("cooldown").Configure("Cooldown", _cooldown, .1f, 4f, data => { _cooldown = data; });
        // Require On Ground
        group.CreateToggle("reqGround").Configure("Requiere On Ground", _requireOnGround, data => { _requireOnGround = data; });
    } 
    public void Use(bool pressed, Vector3 direction)
    {
        if (_timerCooldown > 0)
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
                //_myAnimator.SetTrigger(ANIM_DASH_PREPARATION);
                OnStart?.Invoke();
                SetDirection(direction);
                return;
            }

            if (!_isAiming) return;
            Debug.Log("Pressed False");
            //_myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
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
        _timerDuration = 0;
        _timerCooldown = _cooldown;

        //_myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
        //_myAnimator.SetTrigger(ANIM_DASH_CANCEL);

        OnEnd?.Invoke();
    }
    public void SetDirection(Vector3 direction)
    {
        _inputDir = direction;
    }

    // - UNITY -
    private void Update()
    {
        if (Time.timeScale <= 0) return;

        if (_timerDuration > 0)
        {
            _timerDuration -= Time.deltaTime;

            if (_timerDuration <= 0)
            {
                OnEnd?.Invoke();
            }
            return;
        }

        // Cooldown
        if (_timerCooldown > 0)
        {
            _timerCooldown -= Time.deltaTime;

            if (_timerCooldown > 0) return;

            _timerCooldown = 0;
            return;
        }

        // Aiming
        if (!_isAiming) return;
        HandleRotation(Time.deltaTime);
    }


    // - PRIVATE -
    private void HandleRotation(float deltaTime)
    {
        Quaternion targetRot = Quaternion.LookRotation(_inputDir, Vector3.up);

        _myRigidbody.MoveRotation(targetRot);
    }
    void HandleDash()
    {
        _timerDuration = _duration;
        _timerCooldown = _cooldown;

        // Animation
        //Vector3 animDir = transform.InverseTransformDirection(_inputDir);
        //_myAnimator.SetFloat(ANIM_DASHX, animDir.x);
        //_myAnimator.SetFloat(ANIM_DASHZ, animDir.z);
        //_myAnimator.SetTrigger(ANIM_DASH_USE);

        OnStart?.Invoke();

        _myRigidbody.AddForce(_inputDir * _force, ForceMode.Impulse);
    }
}
