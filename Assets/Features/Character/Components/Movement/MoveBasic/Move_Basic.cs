using UnityEngine;
using UnityEngine.Events;

public class Move_Basic : MonoBehaviour, IMove
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
    private bool _isActive = true;
    private bool _isGrounded = true;

    private const string ANIM_IS_MOVING = "isMoving";
    private const string ANIM_MOVE_X = "moveX";
    private const string ANIM_MOVE_Z = "moveZ";


    // - GET -
    public bool IsActive => _isActive;
    public bool IsRotateActive => false;
    Vector3 BodyPosition => _myRigidbody.transform.position;


    // - PUBLIC -
    public void CreateMeter(RectTransform container)
    {

    }
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Move Basic");

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
    public void SetRotateActive(bool isActive) { }

    public void Move(Vector3 direction)
    {
        _inputDir = direction;

        if (_inputDir == Vector3.zero)
            OnMoveEnd?.Invoke();
        else
            OnMoveStart?.Invoke();
    }        
    public void Rotate(Vector3 diretion) { }

    public void Warp(Vector3 newPosition)
    {
        _inputDir = Vector3.zero;
        _myRigidbody.MovePosition(newPosition);
    }


    // - UNITY -
    void Awake()
    {
        if (_myRigidbody == null)
        {
            _myRigidbody = transform.parent.GetComponent<Rigidbody>();
            if (_myRigidbody == null)
            {
                Debug.LogWarning("Not Rigidbody assigned.", gameObject);
                return;
            }
        }

        _myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    void Update()
    {
        // If time is stopped : return
        if (Time.timeScale <= 0) return;

        HandleAnimation(Time.deltaTime);
    }
    void FixedUpdate()
    {
        if (!_isActive) return;

        if (_inputDir != Vector3.zero)
            HandleRotation(Time.fixedDeltaTime);

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
    private void HandleRotation(float deltaTime)
    {
        Quaternion targetRot = Quaternion.LookRotation(_inputDir, Vector3.up);

        _myRigidbody.MoveRotation(targetRot);
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