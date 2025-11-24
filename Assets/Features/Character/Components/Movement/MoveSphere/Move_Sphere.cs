using UnityEngine;
using UnityEngine.Events;

public class Move_Sphere : MonoBehaviour, IMove
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

    [Header("- Speed -")]
    [SerializeField] private float _groundSpeed = 5.0f;
    [SerializeField] private float _groundMaxSpeed = 8.0f;
    [SerializeField] private float _airSpeed = 2.0f;
    [SerializeField] private float _airMaxSpeed = 4.0f;
    [SerializeField] private float _airSmoothFactor = 4f;

    [Header("- Ground Check -")]
    private float _groundCheckDistance = 0.3f;
    private Vector3 _groundCheckOffset = new Vector3(0, 0.15f, 0);

    [Header("- Not Modify -")]
    private Vector3 _inputDir = Vector3.zero;
    private bool _isActive = true;
    private bool _isGrounded = true;


    // - GET -
    public bool IsActive => _isActive;
    public bool IsRotateActive => false;
    bool IsGrounded => Tool_FloorDetector.HasSomething_Ray(BodyPosition + _groundCheckOffset, Vector3.down, _groundCheckDistance, ~0);
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
        moveSpeed.Configure("Move Speed", _groundSpeed, 0, 20, data => { _groundSpeed = data; });
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
    public void Rotate(Vector3 direction)
    {
        
    }

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
    }
    void FixedUpdate()
    {
        if (!_isActive) return;

        Vector3 force = _inputDir * _groundSpeed;
        _myRigidbody.AddForce(force, ForceMode.Acceleration);

        if (_isGrounded)
        {
            // Clamp duro en suelo
            if (_myRigidbody.linearVelocity.magnitude > _groundMaxSpeed)
            {
                _myRigidbody.linearVelocity = _myRigidbody.linearVelocity.normalized * _groundMaxSpeed;
            }
        }
        else
        {
            // Clamp suave en aire
            if (_myRigidbody.linearVelocity.magnitude > _airMaxSpeed)
            {
                _myRigidbody.linearVelocity = Vector3.Lerp(
                    _myRigidbody.linearVelocity,
                    _myRigidbody.linearVelocity.normalized * _airMaxSpeed,
                    _airSmoothFactor * Time.fixedDeltaTime
                );
            }
        }


    }
}