using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Controller : MonoBehaviour
{
    // - VARIABLES -
    [SerializeField]
    private CinemachineCamera[] _listCameras;

    private List<CinemachineCamera> _availableCameras = new List<CinemachineCamera>();
    private int _cameraSelected = 0;

    private Transform _curTarget;

    PlayerInput _accesInput;
    InputAction _inputViewPrevious;
    InputAction _inputViewNext;
    Vector3 _targetOffset;

    const string INPUT_VIEW_Left = "Btn_L1";
    const string INPUT_VIEW_Right = "Btn_R1";


    // - PUBLIC -
    public void SetTarget(MoveBase_Controller controller, Transform target, Vector3 targetOffset)
    {
        // Set New Target
        _curTarget = target;
        _targetOffset = targetOffset;

        // Obtain Available Cameras
        _availableCameras.Clear();
        List<string> tempAvailables = new List<string>(controller.CamerasAvailable);
        foreach(CinemachineCamera cam in _listCameras)
        {
            cam.Priority.Value = 1;
            int index = tempAvailables.FindIndex(x => cam.name.Contains(x));
            if (index < 0) continue;
            _availableCameras.Add(cam);
            tempAvailables.RemoveAt(index);
        }

        _cameraSelected = 0;
        UpdateCameraData();
    }

    public void ConfigureInput(PlayerInput playerInput)
    {
        if (_accesInput != playerInput)
        {
            if (_accesInput != null)
            {
                RemoveInput();
            }

            _accesInput = playerInput;
            _inputViewPrevious = playerInput.actions[INPUT_VIEW_Left];
            _inputViewNext = playerInput.actions[INPUT_VIEW_Right];
        }

        // Camera
        _inputViewPrevious.performed += Input_View_Previous;
        _inputViewNext.performed += Input_View_Next;
    }
    public void RemoveInput()
    {
        if (_accesInput == null) return;

        _inputViewPrevious.performed -= Input_View_Previous;
        _inputViewNext.performed -= Input_View_Next;

        _accesInput = null;
    }


    // - UNITY -
    private void OnDestroy()
    {
        RemoveInput();
    }


    #region - INPUTS -

    void Input_View_Previous(InputAction.CallbackContext cnt)
    {
        if (_availableCameras.Count == 1) return;

        _availableCameras[_cameraSelected].Priority.Value = 1;

        _cameraSelected--;
        if (_cameraSelected < 0)
            _cameraSelected = _availableCameras.Count - 1;

        UpdateCameraData();
    }
    void Input_View_Next(InputAction.CallbackContext cnt)
    {
        if (_availableCameras.Count == 1) return;

        _availableCameras[_cameraSelected].Priority.Value = 1;

        _cameraSelected++;
        if (_cameraSelected >= _availableCameras.Count)
            _cameraSelected = 0;

        UpdateCameraData();
    }

    #endregion


    // - PRIVATE -
    void UpdateCameraData()
    {
        _availableCameras[_cameraSelected].Follow = _curTarget;
        _availableCameras[_cameraSelected].Priority.Value = 5;

        CinemachineRotationComposer rotComposer = _availableCameras[_cameraSelected].GetComponent<CinemachineRotationComposer>();
        if (rotComposer != null)
            rotComposer.TargetOffset = _targetOffset;
    }



    /*
         [Header("- Camera Offset -")]
    [SerializeField] CinemachineFollow _cameraFollow;
    [SerializeField] Vector3[] _listCamOffsets = new Vector3[] { 
        new Vector3(-6, 10, -6),
        new Vector3(-6, 10, 6), 
        new Vector3(6, 10, 6), 
        new Vector3(6, 10, -6)
    };
    int _selectedCamOffset = 0;

      InputAction _inputViewPrevious;
    InputAction _inputViewNext;

        const string INPUT_VIEW_Left = "Btn_L1";
    const string INPUT_VIEW_Right = "Btn_R1";

        // Camera
    void Input_View_Previous(InputAction.CallbackContext cnt)
    {
        if (!_canControl) return;

        _selectedCamOffset--;
        if (_selectedCamOffset < 0)
            _selectedCamOffset = _listCamOffsets.Length - 1;

        UpdateCameraOffset();
    }
    void Input_View_Next(InputAction.CallbackContext cnt)
    {
        if (!_canControl) return;

        _selectedCamOffset++;
        if (_selectedCamOffset >= _listCamOffsets.Length)
            _selectedCamOffset = 0;

        UpdateCameraOffset();
    }

        void UpdateCameraOffset()
    {
        _cameraFollow.FollowOffset = _listCamOffsets[_selectedCamOffset];
    }
     
     */
}