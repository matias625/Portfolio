using UnityEngine;
using UnityEngine.InputSystem;

public abstract class MoveBase_Controller : MonoBehaviour
{
    // - VARIABLES -
    [Header("- Camera Options -")]
    [SerializeField, Tooltip("Cameras available to use")]
    protected string[] _availabeCameras = new string[] { "Isometric" };
    [SerializeField, Tooltip("Use camera to calculate direction on inputs.")]
    protected bool _useCameraView = true;


    // - GET -
    public abstract string[] CamerasAvailable { get; }

    // - PUBLIC -
    public abstract void SetControl(bool isActive);
    public void SetCameraOptions(bool useCameraView)
    {
        _useCameraView = useCameraView;
    }
    public abstract void SetCharacter(Move_Control character);
    public abstract void ConfigureInput(PlayerInput playerInput);
    public abstract void RemoveInput();

    // - PRIVATE -
    protected Vector3 Direction_ByCamera(Vector2 _input, Camera camera)
    {
        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        //this is the direction in the world space we want to move:
        return forward * _input.y + right * _input.x;
    }
}