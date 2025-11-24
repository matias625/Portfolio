using UnityEngine;
using UnityEngine.Events;

public class Move_Aim : MonoBehaviour, IMove
{
    // - EVENTS -
    public event UnityAction OnMoveStart;
    public event UnityAction OnMoveEnd;

    public void Event_OnGroundChange(bool isGrounded)
    {
        _isGrounded = isGrounded;
    }

    // - VARIABLES -
    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _myAnimator;

    [Header("- Options -")]
    [SerializeField, Tooltip("Speed of animation to change")]
    private float _moveAnimToZeroMultiplier = 6;

    [Header("- Speed -")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _airSpeed = 2.0f;
    [SerializeField] private float _airSmoothFactor = 4f;

    [Header("- Not Modify -")]
    private Vector3 _inputDir = Vector3.zero;
    private Vector3 _inputRotate = Vector3.zero;
    private bool _isActive = true;
    private bool _isRotateActive = true;
    private bool _isGrounded = true;

    private const string ANIM_IS_MOVING = "isMoving";
    private const string ANIM_MOVE_X = "moveX";
    private const string ANIM_MOVE_Z = "moveZ";


    // - GET -
    public bool IsActive => _isActive;
    public bool IsRotateActive => _isRotateActive;
    Vector3 BodyPosition => _myRigidbody.transform.position;

    // - PUBLIC -
    public void CreateMeter(RectTransform container)
    {

    }
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Move Aim");

        // Move Speed
        Slot_StatSlider moveSpeed = group.CreateSlider("moveSpeed");
        moveSpeed.Configure("Move Speed", _moveSpeed, 0, 20, data => { _moveSpeed = data; });
        // Air Speed
        Slot_StatSlider airSpeed = group.CreateSlider("airSpeed");
        airSpeed.Configure("Air Speed", _airSpeed, 0, 20, data => { _airSpeed = data; });
        // Air Smooth Factor
        Slot_StatSlider airSmooth = group.CreateSlider("airSmooth");
        airSmooth.Configure("Air Smooth Factor", _airSmoothFactor, 0, 20, data => { _airSmoothFactor = data; });
    }

    public void SetActive(bool isActive) => _isActive = isActive;
    public void SetRotateActive(bool isActive) => _isRotateActive = isActive;

    public void Move(Vector3 direction)
    {
        _inputDir = direction;

        if (_inputDir == Vector3.zero)
            OnMoveEnd?.Invoke();
        else
            OnMoveStart?.Invoke();
    }
    public void Rotate(Vector3 direction) => _inputRotate = direction;

    public void Warp(Vector3 newPosition)
    {
        _inputDir = Vector3.zero;
        _myRigidbody.MovePosition(newPosition);
    }


    // - UNITY -
    private void Update()
    {
        if (Time.timeScale == 0) return;

        if (_isRotateActive)
            HandleRotation();

        HandleAnimation(Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (!_isActive) return;

        Vector3 currentVel = _myRigidbody.linearVelocity;

        if (_isGrounded)
        {
            currentVel.x = _inputDir.x * _moveSpeed;
            currentVel.z = _inputDir.z * _moveSpeed;
        }
        else
        {
            Vector3 targetVel = new Vector3(_inputDir.x * _airSpeed, currentVel.y, _inputDir.z * _airSpeed);
            currentVel = Vector3.Lerp(currentVel, targetVel, Time.fixedDeltaTime * _airSmoothFactor);
        }

        _myRigidbody.linearVelocity = currentVel;
    }


    // - PRIVATE -
    private void HandleRotation()
    {
        if (_inputRotate == Vector3.zero) return;

        _myRigidbody.MoveRotation(Quaternion.LookRotation(_inputRotate));
    }
    private void HandleAnimation(float deltaTime)
    {
        Vector3 animDir = transform.InverseTransformDirection(_myRigidbody.linearVelocity);

        float curX = Mathf.Lerp(_myAnimator.GetFloat(ANIM_MOVE_X), animDir.x, deltaTime * _moveAnimToZeroMultiplier);
        float curZ = Mathf.Lerp(_myAnimator.GetFloat(ANIM_MOVE_Z), animDir.z, deltaTime * _moveAnimToZeroMultiplier);

        _myAnimator.SetFloat(ANIM_MOVE_X, Mathf.Clamp(curX, -1, 1));
        _myAnimator.SetFloat(ANIM_MOVE_Z, Mathf.Clamp(curZ, -1, 1));
        _myAnimator.SetBool(ANIM_IS_MOVING, curX != 0 || curZ != 0);
    }
}