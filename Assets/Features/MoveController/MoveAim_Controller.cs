using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Move_Aim & Move_Boat
/// Cameras: Isometric
/// </summary>
public class MoveAim_Controller : MoveBase_Controller
{
    // - EVENTS -
    void Input_OnDeviceChange(PlayerInput _input)
    {
        if (!_canControl)
        {
            _pointerArrow.gameObject.SetActive(false);
        }

        if (_accesCharacter != null)
        {
            _pointerArrow.gameObject.SetActive(true);
            Transform tempTransform = _accesCharacter.transform;

            _pointerArrow.position = tempTransform.position + (tempTransform.forward * 1);
            _pointerArrow.rotation = tempTransform.rotation;
        }
        else
        {
            _pointerArrow.gameObject.SetActive(false);
            _pointerArrow.position = Vector3.zero;
        }
    }

    // - VARIABLES -
    [Header("- Pointer Arrow -")]
    [SerializeField, Tooltip("Character will look at it, if can.")]
    private Transform _pointerArrow;
    Plane _groundPlane;
    float _cooldownMovePlane = .5f;
    float _timerMovePlane = 0f;

    Move_Control _accesCharacter;
    Camera _myCamera;
    bool _canControl = true;

    PlayerInput _accesInput;
    InputAction _inputMove;
    InputAction _inputRotate;
    InputAction _inputDash;
    InputAction _inputJump;
    Vector2 _mousePosition;

    const string INPUT_MOVE = "Stick_Left";
    const string INPUT_ROTATE = "Stick_Right";
    const string INPUT_DASH = "Btn_R2";
    const string INPUT_JUMP = "Btn_South";

    // - GET -
    public override string[] CamerasAvailable => _availabeCameras;
    bool IsWorking => _canControl && _accesCharacter != null;

    // - OVERRIDES -
    public override void SetControl(bool isActive) => _canControl = isActive;
    public override void SetCharacter(Move_Control character)
    {
        if (_accesCharacter != null)
            _accesCharacter.SetSelected(false);

        _accesCharacter = character;
        _accesCharacter.SetSelected(true);
    }
    public override void ConfigureInput(PlayerInput playerInput)
    {
        if (_accesInput != playerInput)
        {
            if (_accesInput != null)
                RemoveInput();
            
            _accesInput = playerInput;
            _inputMove = playerInput.actions[INPUT_MOVE];
            _inputRotate = playerInput.actions[INPUT_ROTATE];
            _inputDash = playerInput.actions[INPUT_DASH];
            _inputJump = playerInput.actions[INPUT_JUMP];
        }

        // - - Set Actions
        // Move
        _inputMove.performed += Input_Move_Performed;
        _inputMove.canceled += Input_Move_Cancel;
        // Rotate
        _inputRotate.performed += Input_Rotate_Performed;
        // Dash
        _inputDash.performed += Input_Dash_Performed;
        // Jump
        _inputJump.performed += Input_Jump_Performed;

        // Event
        _accesInput.controlsChangedEvent.AddListener(Input_OnDeviceChange);
    }
    public override void RemoveInput()
    {
        if (_accesInput == null) return;

        // Move
        _inputMove.performed -= Input_Move_Performed;
        _inputMove.canceled -= Input_Move_Cancel;
        // Rotate
        _inputRotate.performed -= Input_Rotate_Performed;
        // Dash
        _inputDash.performed -= Input_Dash_Performed;
        // Jump
        _inputJump.performed -= Input_Jump_Performed;

        // Event
        _accesInput.controlsChangedEvent.RemoveListener(Input_OnDeviceChange);

        _accesInput = null;
    }


    // - UNITY -
    private void Awake()
    {
        // Generate new Ground
        _groundPlane = new Plane(Vector3.up, Vector3.zero);
    }
    private void Start()
    {
        if (_myCamera == null)
        {
            Debug.Log("Setting new camera", gameObject);
            _myCamera = Camera.main;
        }
    }
    private void Update()
    {
        if (!IsWorking) return;
        if (!_pointerArrow.gameObject.activeSelf) return;

        _timerMovePlane -= Time.deltaTime;
        if (_timerMovePlane <= 0)
        {
            _groundPlane.SetNormalAndPosition(Vector3.up, _accesCharacter.transform.position);
            _timerMovePlane = _cooldownMovePlane;
        }

        Ray ray = _myCamera.ScreenPointToRay(_mousePosition); 

        if (_groundPlane.Raycast(ray, out float rayLength))
        {
            Vector3 pointToLook = ray.GetPoint(rayLength);
            Vector3 direction = pointToLook - _accesCharacter.transform.position;

            if ((direction.x > -.1f && direction.x < .1f) &&
                (direction.z > -.1f && direction.z < .1f))
            {
                return;
            }

            direction.Normalize();

            // Send Rotation to Character
            _accesCharacter.Rotate(new Vector3(direction.x, 0, direction.z));

            // Set Arrow Pos & Rot
            _pointerArrow.position = pointToLook;
            _pointerArrow.rotation = _accesCharacter.transform.rotation;
        }
    }


    // - INPUTS -
    void Input_Move_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        if (_useCameraView)
        {
            _accesCharacter.Move(Direction_ByCamera(cnt.ReadValue<Vector2>(), _myCamera));
            return;
        }
        Vector2 curDir = cnt.ReadValue<Vector2>();
        _accesCharacter.Move(new Vector3(curDir.x, 0, curDir.y));
    }
    void Input_Move_Cancel(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Move(Vector3.zero);
    }
    void Input_Rotate_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        if (cnt.control.device == Mouse.current)
        {
            _mousePosition = cnt.ReadValue<Vector2>();
        }
        else
        {
            _accesCharacter.Rotate(Direction_ByCamera(cnt.ReadValue<Vector2>(), _myCamera));
        }
    }
    void Input_Dash_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Dash(true);
    }
    void Input_Jump_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Jump();
    }
}