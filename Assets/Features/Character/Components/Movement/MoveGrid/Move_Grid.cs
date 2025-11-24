using UnityEngine;
using UnityEngine.Events;

public class Move_Grid : MonoBehaviour, IMove
{
    // - EVENTS -
    public event UnityAction OnMoveStart;
    public event UnityAction OnMoveEnd;

    public void Event_OnGroundChange(bool isGrounded)
    {
        // _isGrounded = isGrounded;
    }

    // - VARIABLES -
    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _myAnimator;

    [Header("- Options -")]
    [SerializeField] private LayerMask _obstacleMask;    // Máscara para detectar obstáculos
    [SerializeField] private bool _allowDiagonalMovement = true;
    [SerializeField, Tooltip("Speed of animation to change")]
    private float _moveAnimToZeroMultiplier = 6;
    // [SerializeField] private float _characterHeight = 1f;

    [Header("- Speed -")]
    [SerializeField] private float _moveSpeed = 5.0f;

    [Header("- Rotate -")]
    [SerializeField] private bool _rotateOverTime = true;
    [SerializeField] private float _rotateSpeed = 10f;

    [Header("- Not Modify -")]
    private Vector3 _inputDir = Vector3.zero;
    private Vector3 _desiredDir = Vector3.zero;
    private Vector3 _targetPosition;
    private Vector3 _previousPosition;

    private bool _isActive = true;
    private bool _isMoving = false;
    private float _timerMove = 0;
    private const float _timerMoveRequired = 5f;

    private const string ANIM_MOVE_Z = "moveZ";


    // - GET -
    public bool IsActive => _isActive;
    public bool IsRotateActive => false;
    Vector3 BodyPosition => _myRigidbody.position;


    // - PUBLIC -
    public void CreateMeter(RectTransform container)
    {

    }
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Move Grid");
        // Diagonal Movement
        group.CreateToggle("diagMovement").Configure("Allow Diagonal Movement", _allowDiagonalMovement, data => { _allowDiagonalMovement = data; });
        // Move Speed
        group.CreateSlider("moveSpeed").Configure("Move Speed", _moveSpeed, 0, 20, data => { _moveSpeed = data; });
        // Rotate Over Time
        group.CreateToggle("rotateOT").Configure("Rotate Over Time", _rotateOverTime, data => { _rotateOverTime = data; });
        // Rotate Speed
        group.CreateSlider("rotSpeed").Configure("Rotate Speed", _rotateSpeed, 0, 90, data => { _rotateSpeed = data; });
    }
    public void SetActive(bool isActive)
    {
        if (!isActive)
        {
            _inputDir = Vector3.zero;
            _isMoving = false;
        }            

        // update position
        _previousPosition = transform.position;
        _targetPosition = transform.position;

        _isActive = isActive;
    }
    public void SetRotateActive(bool isActive) { }

    public void Move(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            _inputDir = Vector3.zero;
            return;
        }

        _inputDir = _allowDiagonalMovement
            ? new Vector3(Mathf.Round(direction.x), Mathf.Round(direction.y), Mathf.Round(direction.z))
            : GetAxisLockedDirection(direction);

        _desiredDir = _inputDir;
    }
    public void Rotate(Vector3 direction) { }

    public void Warp(Vector3 newPosition)
    {
        _inputDir = Vector3.zero;
        _isMoving = false;
        _timerMove = 0;

        _targetPosition = GridManager.Instance.GetClosestCellCenter(newPosition);
        _previousPosition = _targetPosition;

        _myRigidbody.MovePosition(_targetPosition);
    }


    // - UNITY -
    private void Awake()
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
    private void Start()
    {
        _previousPosition = GridManager.Instance.GetClosestCellCenter(transform.position);
        _targetPosition = _previousPosition;
        _myRigidbody.MovePosition(_targetPosition);
    }
    private void Update()
    {
        if (Time.timeScale <= 0) return;

        float deltaTime = Time.deltaTime;

        if (_isActive && !_isMoving && _desiredDir != Vector3.zero)
        {
            if (HandleRotation(deltaTime) && _inputDir != Vector3.zero)
                TryStartStep();
        }

        if (_myAnimator != null && !_myRigidbody.isKinematic)
            HandleAnimation(deltaTime);
    }
    private void FixedUpdate()
    {
        if (!_isActive || !_isMoving) return; // || !IsGrounded) return;

        float fixDeltaTime = Time.fixedDeltaTime;
        Vector3 dir = (_targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, _targetPosition);
        float step = _moveSpeed * fixDeltaTime;

        _timerMove += fixDeltaTime;
        if (_timerMove > _timerMoveRequired)
        {
            _targetPosition = _previousPosition;
            _timerMove = 0;
        }

        if (step >= distance)
        {
            _myRigidbody.MovePosition(_targetPosition);

            if (_inputDir == Vector3.zero)
            {
                _isMoving = false;
                OnMoveEnd?.Invoke();
                return;
            }

            if (!HandleRotation(fixDeltaTime))
                return;

            TryStartStep();
        }
        if (_isMoving)
        {
            _myRigidbody.MovePosition(transform.position + (dir * step));
        }
    }


    // - PRIVATE -
    private bool HandleRotation(float deltaTime)
    {
        if (_desiredDir == Vector3.zero) return false;

        Quaternion targetRot = Quaternion.LookRotation(_desiredDir, Vector3.up);

        if (!_rotateOverTime || Vector3.Angle(_myRigidbody.transform.forward, _desiredDir) < 0.1f)
        {
            _myRigidbody.MoveRotation(targetRot);
            return true;
        }

        _myRigidbody.MoveRotation(Quaternion.Lerp(_myRigidbody.rotation, targetRot, _rotateSpeed * deltaTime));
        return false;
    }
    private void HandleAnimation(float deltaTime)
    {
        float animDir = _isMoving ? 1f : 0;

        float curZ = Mathf.SmoothDamp(_myAnimator.GetFloat(ANIM_MOVE_Z),
            animDir,
            ref _moveAnimToZeroMultiplier,
            .15f,
            10, deltaTime);

        // float curZ = Mathf.Lerp(_myAnimator.GetFloat(ANIM_MOVE_Z), animDir, deltaTime * _moveAnimToZeroMultiplier);
        _myAnimator.SetFloat(ANIM_MOVE_Z, Mathf.Clamp(curZ, -1, 1));
    }
    private void TryStartStep()
    {
        Vector3Int potentialCell = GridManager.Instance.GetCellInDirection(_targetPosition, _inputDir);

        // Check Obstacle
        if (Tool_FloorDetector.HasSomething_Ray(transform.position + (Vector3.up * .5f), _inputDir, 1, _obstacleMask))  //!GridManager.Instance.IsCellFree(potentialCell))
        {
            Debug.Log($"Obstacle on cell : {potentialCell} -Position: {GridManager.Instance.CellToWorld(potentialCell)}");
            _isMoving = false;
            return;
        }
        // Check Floor
        if (!GridManager.Instance.HasCellFloor(potentialCell))
        {
            Debug.Log($"No Floor on cell : {potentialCell} -Position: {GridManager.Instance.CellToWorld(potentialCell)}");
            _isMoving = false;
            return;
        }

        Vector3 nextPosition = GridManager.Instance.CellToWorld(potentialCell);

        // Set Previous Position
        _previousPosition = _targetPosition;
        _timerMove = 0;

        // Set New Target Position
        _targetPosition = nextPosition; // potentialPosition;
        _isMoving = true;
        OnMoveStart?.Invoke();
    }

    private Vector3 GetAxisLockedDirection(Vector3 dir)
    {
        // Elige el eje dominante
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) && Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.z))
            return new Vector3(0, Mathf.Sign(dir.y), 0);
        return new Vector3(0, 0, Mathf.Sign(dir.z));
    }
}