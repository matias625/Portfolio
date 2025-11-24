using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Move_Grid & Move_Basic 
/// Cameras: Isometric, Orbital
/// </summary>
public class MoveBasic_Controller : MoveBase_Controller
{
    // - VARIABLES -
    Move_Control _accesCharacter;
    Camera _myCamera;
    bool _canControl = true;

    PlayerInput _accesInput;
    InputAction _inputMove;
    InputAction _inputDash;
    InputAction _inputJump;
  
    const string INPUT_MOVE = "Stick_Left";
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
            {
                RemoveInput();
            }

            _accesInput = playerInput;
            _inputMove = playerInput.actions[INPUT_MOVE];
            _inputDash = playerInput.actions[INPUT_DASH];
            _inputJump = playerInput.actions[INPUT_JUMP];
        }

        // - - Set Actions
        // Move
        _inputMove.performed += Input_Move_Performed;
        _inputMove.canceled += Input_Move_Cancel;
        // Dash
        _inputDash.performed += Input_Dash_Performed;
        _inputDash.canceled += Input_Dash_Cancel;
        // Jump
        _inputJump.performed += Input_Jump_Performed;
    }
    public override void RemoveInput()
    {
        if (_accesInput == null) return;
        // Move
        _inputMove.performed -= Input_Move_Performed;
        _inputMove.canceled -= Input_Move_Cancel;
        // Dash
        _inputDash.performed -= Input_Dash_Performed;
        _inputDash.canceled -= Input_Dash_Cancel;
        // Jump
        _inputJump.performed -= Input_Jump_Performed;

        _accesInput = null;
    }


    // - UNITY -
    private void Start()
    {
        if (_myCamera == null)
        {
            Debug.Log("Setting new camera", gameObject);
            _myCamera = Camera.main;
        }
    }


    #region - INPUTS -

    // Move
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
    // Dash
    void Input_Dash_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Dash(true);
    }
    void Input_Dash_Cancel(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Dash(false);
    }
    // Jump
    void Input_Jump_Performed(InputAction.CallbackContext cnt)
    {
        if (!IsWorking) return;

        _accesCharacter.Jump();
    }

    #endregion


    // - PRIVATE -
}