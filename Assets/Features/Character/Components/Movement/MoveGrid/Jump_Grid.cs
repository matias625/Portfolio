using UnityEngine;
using UnityEngine.Events;

public class Jump_Grid : MonoBehaviour, IJump
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
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _characterHeight = 1f;

    [Header("- Jump Settings -")]
    [SerializeField, Range(1, 10)] 
    private int _maxForwardSteps = 2;
    [SerializeField, Range(0, 8)]
    private int _maxUpSteps = 1;
    [SerializeField, Range(0, 8)]
    private int _maxDownSteps = 2;
    [SerializeField] private float _jumpDuration = 0.5f;
    [SerializeField] private float _jumpHeight = 1.5f;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private bool _isJumping = false;
    private float _jumpTimer = 0f;

    private const string ANIM_JUMP = "jump";
    private const string ANIM_LAND = "land";


    // - GET -
    public bool CanMoveOnAir => false;


    // - PUBLIC -
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Jump Grid");
        // Steps
        group.CreateSlider("maxForwardSteps").Configure("Max Forward Steps", _maxForwardSteps, 1, 10, data => { _maxForwardSteps = Mathf.RoundToInt(data); }, true);
        group.CreateSlider("maxUpSteps").Configure("Max Up Steps", _maxUpSteps, 1, 10, data => { _maxUpSteps = Mathf.RoundToInt(data); }, true);
        group.CreateSlider("maxDownSteps").Configure("Max Down Steps", _maxDownSteps, 1, 10, data => { _maxDownSteps = Mathf.RoundToInt(data); }, true);
        // Jump 
        group.CreateField("jumpDur").Configure("Jump Duration", _jumpDuration.ToString(), data => _jumpDuration = data);
        group.CreateField("jumpDur").Configure("Jump Height", _jumpHeight.ToString(), data => _jumpHeight = data);
    }
    public void Use(Vector3 direction)
    {
        if (_isJumping || _jumpTimer > 0) return;

        // Obtain Current & Forward Cell
        Vector3Int currentCell = GridManager.Instance.WorldToCell(_myRigidbody.transform.position);
        Vector3 forwardDir = transform.forward;
        Vector3Int forwardCellOffset = new Vector3Int(
            Mathf.RoundToInt(forwardDir.x),
            Mathf.RoundToInt(forwardDir.y),
            Mathf.RoundToInt(forwardDir.z)
        );

        // Obtain Target Cell
        Vector3Int targetCell = currentCell;
        for (int f = 1; f <= _maxForwardSteps; f++)
        {
            bool finished = false;

            for (int u = -_maxDownSteps; u <= _maxUpSteps - f + 1; u++)
            {
                Vector3Int candidate = currentCell + (forwardCellOffset * f) + (Vector3Int.up * u);

                if (GridManager.Instance.IsCellFree(candidate) 
                    && GridManager.Instance.HasCellFloor(candidate)
                    && GridManager.Instance.IsJumpTrajectoryClear(currentCell, candidate, _jumpHeight, _characterHeight, 10))
                {
                    targetCell = candidate;
                    if (u != 0)
                    {
                        finished = true;
                        break;
                    }
                }
            }

            if (finished)
                break;
        }

        // If current & target are same = return
        if (targetCell == currentCell) return;

        // Convert to World Positions
        _startPos = GridManager.Instance.CellToWorld(currentCell);
        _endPos = GridManager.Instance.CellToWorld(targetCell);

        // Debug.Log($"StartPos:{_startPos} | EndPos:{_endPos}");

        // Reset Jump Options
        _jumpTimer = 0f;
        _isJumping = true;

        // Use Event
        OnStart?.Invoke();

        // Set Animation
        if (_myAnimator == null) return;
        _myAnimator.ResetTrigger(ANIM_LAND);
        _myAnimator.SetTrigger(ANIM_JUMP);
    }


    // - UNITY -
    private void FixedUpdate()
    {
        // If NOT jumping : return
        if (!_isJumping) return;

        // Calculate Time
        _jumpTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_jumpTimer / _jumpDuration);

        // Parabolic Interpolation
        Vector3 pos = Vector3.Lerp(_startPos, _endPos, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * _jumpHeight;

        // Apply Position
        _myRigidbody.MovePosition(pos);

        if (t < 1f) return;

        _isJumping = false;
        _jumpTimer = 0f;
        if (_myAnimator != null)
            _myAnimator.SetTrigger(ANIM_LAND);

        OnEnd?.Invoke();
    }


    // - PRIVATE -
}