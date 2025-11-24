using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 
/// </summary>
public class Dash_Grid : MonoBehaviour, IDash
{
    // - EVENTS -
    public event UnityAction OnStart;
    public event UnityAction OnEnd;

    public void Event_OnGroundChange(bool isGrounded)
    {
        
    }


    // - VARIABLES -
    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _myAnimator;

    [Header("- Options -")]
    [SerializeField] private LayerMask _obstacleMask;    // Máscara para detectar obstáculos
    [SerializeField] private bool _allowDiagonalMovement = true;
    [SerializeField] private float _characterHeight = 1f;

    [Header("- Speed -")]
    [SerializeField] private int _dashSteps = 5;
    [SerializeField] private float _moveSpeed = 10.0f;

    [Header("- Rotate -")]
    [SerializeField] private bool _rotateOverTime = true;
    [SerializeField] private float _rotateSpeed = 20f;

    private Vector3 _desiredDir = Vector3.zero;
    private Vector3 _targetPosition = Vector3.zero;
    private bool _isAiming = false;
    private bool _isMoving = false;
    private bool _pendingDash = false;

    private const string ANIM_DASH_PREPARATION = "dashPrep";
    private const string ANIM_DASH_CANCEL = "dashCancel";
    private const string ANIM_DASH_USE = "dashUse";


    // - GET -
    public bool IsAiming => _isAiming;
    public bool IsMoving => _isMoving;
    /// <summary>
    /// If is NOT moving, can jump
    /// </summary>
    public bool CanJump => !_isMoving;
    bool IsGrounded => GridManager.Instance.HasWorldFloor(BodyPosition);
    Vector3 BodyPosition => _myRigidbody.position;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Dash Grid");

        // Diagonal Movement
        group.CreateToggle("diagMovement").Configure("Allow Diagonal Movement", _allowDiagonalMovement, data => { _allowDiagonalMovement = data; });
        // Steps/Move
        group.CreateSlider("dashSteps").Configure("Dash Steps", _dashSteps, 1, 20, data => { _dashSteps = Mathf.RoundToInt(data); }, true);
        group.CreateSlider("moveSpeed").Configure("Move Speed", _moveSpeed, 0, 20, data => { _moveSpeed = data; });
        // Rotate
        group.CreateToggle("rotateOT").Configure("Rotate Over Time", _rotateOverTime, data => { _rotateOverTime = data; });
        group.CreateSlider("rotSpeed").Configure("Rotate Speed", _rotateSpeed, 0, 90, data => { _rotateSpeed = data; });
    }
    public void Use(bool pressed, Vector3 direction)
    {
        if (pressed)
        {
            // Debug.Log($"Dash pressed:{pressed} direction: {direction}");
            if (direction == Vector3.zero) return;
            if (!IsGrounded) return;

            // Try Enter Aim Mode
            Vector3 snapped = GridManager.Instance.GetClosestCellCenter(BodyPosition);
            if (Vector3.Distance(BodyPosition, snapped) < 0.05f && !_isMoving)
            {
                EnterAimMode();
            }
            else
            {
                // Estoy en movimiento → esperar hasta llegar a celda
                _pendingDash = true;
            }

            SetDirection(direction);
            return;
        }

        // If was not aiming : return
        if (!_isAiming) return;
        // Debug.Log($"Dash pressed:{pressed} desired direction: {_desiredDir}, target position: {_targetPosition}");
        if (_desiredDir == Vector3.zero) return;

        CheckSteps(_desiredDir, _dashSteps);

        // Use Dash
        _isAiming = false;
        _isMoving = true;
        _myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
        _myAnimator.SetTrigger(ANIM_DASH_USE);
    }
    public void Cancel()
    {
        _isAiming = false;
        _isMoving = false;

        _myAnimator.ResetTrigger(ANIM_DASH_PREPARATION);
        _myAnimator.SetTrigger(ANIM_DASH_CANCEL);

        OnEnd?.Invoke();
    }
    public void SetDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        if (_allowDiagonalMovement)
        {
            _desiredDir = new Vector3(
                Mathf.Round(direction.x),
                Mathf.Round(direction.y),
                Mathf.Round(direction.z)
            );
        }
        else
        {
            // elegir el eje dominante
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                _desiredDir = new Vector3(Mathf.Sign(direction.x), 0, 0);
            else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.z))
                _desiredDir = new Vector3(0, Mathf.Sign(direction.y), 0);
            else
                _desiredDir = new Vector3(0, 0, Mathf.Sign(direction.z));
        }
    }

  
    // - UNITY -
    private void Awake()
    {
        
    }
    private void Update()
    {
        if (Time.timeScale <= 0) return;
                
        if (_pendingDash && !_isMoving)
        {
            Vector3 snapped = GridManager.Instance.GetClosestCellCenter(BodyPosition);
            if (Vector3.Distance(BodyPosition, snapped) < 0.05f)
            {
                _pendingDash = false;
                EnterAimMode();
            }
        }

        if (!_isAiming) return;
        if (!HandleRotation(Time.deltaTime)) return;

        CheckSteps(_desiredDir, _dashSteps);
    }
    void FixedUpdate()
    {
        if (!_isMoving) return;

        float fixDeltaTime = Time.fixedDeltaTime;
        Vector3 dir = (_targetPosition - BodyPosition).normalized;
        float distance = Vector3.Distance(BodyPosition, _targetPosition);
        float step = _moveSpeed * fixDeltaTime;

        if (step >= distance)
        {
            _myRigidbody.MovePosition(_targetPosition);
            _isMoving = false;
            OnEnd?.Invoke();
            return;
        }

        _myRigidbody.MovePosition(BodyPosition + (dir * step));
    }


    // - PRIVATE -
    bool HandleRotation(float deltaTime)
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
    void CheckSteps(Vector3 direction, int steps)
    {
        Vector3 potentialPosition = BodyPosition;
        float halfHeight = _characterHeight / 2f;
        Vector3Int currentCell = GridManager.Instance.WorldToCell(BodyPosition);

        for (int i = 1; i < steps; i++)
        {
            Vector3Int candidate = GridManager.Instance.GetCellInDirection(BodyPosition, direction, i);

            if (!GridManager.Instance.IsCellFree(candidate)) break;
            if (!GridManager.Instance.HasCellFloor(candidate)) break;
            
            potentialPosition = GridManager.Instance.CellToWorld(candidate);
        }

        _targetPosition = potentialPosition;
    }
    private void EnterAimMode()
    {  
        // Reset Position
        _targetPosition = BodyPosition;
        OnStart?.Invoke();
        _isAiming = true;
        _myAnimator.SetTrigger(ANIM_DASH_PREPARATION);
    }
}