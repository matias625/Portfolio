using UnityEngine;

public class Move_Control : MonoBehaviour
{
    // - EVENTS -
    void Event_OnDash_Start()
    {
        _linkMove.SetActive(false);
    }
    void Event_OnDash_End()
    {
        _linkMove.SetActive(true);
        _linkMove.Move(_lastMoveDirMaked);
    }

    void Event_OnJump_Start()
    {
        if (!_linkJump.CanMoveOnAir)
            _linkMove.SetActive(false);
    }
    void Event_OnJump_End()
    {
        if (!_linkJump.CanMoveOnAir)
            _linkMove.SetActive(true);

        _linkMove.Move(_lastMoveDirMaked);
    }


    // - VARIABLES -
    [SerializeField] private Rigidbody _myRigidbody;

    [SerializeField] private GameObject _accesMove;
    [SerializeField] private GameObject _accesDetectGround;
    [SerializeField] private GameObject _accesDash;
    [SerializeField] private GameObject _accesJump;

    [Header("- Visual -")]
    [SerializeField] private GameObject _goSelected;

    // Must not be Zero
    private Vector3 _lastMoveDir = Vector3.zero;
    // Can be Zero
    private Vector3 _lastMoveDirMaked = Vector3.zero;

    private IMove _linkMove;
    private IDash _linkDash;
    private IJump _linkJump;
    private IDetectGround _linkDetectGround;


    // - PUBLIC -
    public void ShowMeters(RectTransform container)
    {
        if (container == null) return;

        Tool_Cleaner.Clean_Transform(container);

        if (_linkMove != null)
            _linkMove.CreateMeter(container);
    }
    public void ShowStats(Panel_Stats panelStats)
    {
        if (panelStats == null) return;

        panelStats.CleanAll();

        if (_linkMove != null)
            _linkMove.CreateGroup(panelStats);
        if (_linkDash != null)
            _linkDash.CreateGroup(panelStats);
        if (_linkJump != null)
            _linkJump.CreateGroup(panelStats);
    }

    public void SetMoveActive(bool active)
    {
        _linkMove.SetActive(active);
    }

    public void SetSelected(bool active)
    {
        _goSelected.SetActive(active);
    }
    public void Move(Vector3 direction)
    {
        // If current direction != zero : save it like current direction
        if (direction != Vector3.zero)
            _lastMoveDir = direction;
        _lastMoveDirMaked = direction;

        if (_linkDash != null && _linkDash.IsAiming)
        {
            _linkDash.SetDirection(direction);
            return;
        }
        _linkMove.Move(direction);
    }
    public void Rotate(Vector3 dir)
    {
        if (!_linkMove.IsRotateActive) return;

        if (_linkDash != null && _linkDash.IsAiming)
        {
            _linkDash.SetDirection(dir);
        }
        _linkMove.Rotate(dir);  
    }
 
    public void Dash(bool pressed)
    {
        if (_linkDash == null) return;

        _linkDash.Use(pressed, _lastMoveDir);
    }

    public void Jump()
    {
        if (_linkJump == null) return;
        if (_linkDash != null)
        {
            if (!_linkDash.CanJump) return;
            if (_linkDash.IsAiming)
                _linkDash.Cancel();
        }

        _linkJump.Use(_lastMoveDir);
    }


    // - UNITY -
    private void Awake()
    {
        _linkMove = _accesMove.GetComponent<IMove>();        
        _linkDash = _accesDash.GetComponent<IDash>();
        _linkJump = _accesJump.GetComponent<IJump>();
        if (_accesDetectGround != null)
            _linkDetectGround = _accesDetectGround.GetComponent<IDetectGround>();

        // Events
        if (_linkDash != null)
        {
            _linkDash.OnStart += Event_OnDash_Start;
            _linkDash.OnEnd += Event_OnDash_End;
        }
        
        if (_linkJump != null)
        {
            _linkJump.OnStart += Event_OnJump_Start;
            _linkJump.OnEnd += Event_OnJump_End;
        }

        if (_linkDetectGround != null)
        {
            if (_linkMove != null)
                _linkDetectGround.OnGroundChange += _linkMove.Event_OnGroundChange;
            if (_linkDash != null)
                _linkDetectGround.OnGroundChange += _linkDash.Event_OnGroundChange;
            if (_linkJump != null)
                _linkDetectGround.OnGroundChange += _linkJump.Event_OnGroundChange;
        }

        _lastMoveDir = transform.forward;
    }
}