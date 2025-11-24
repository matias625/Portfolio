using UnityEngine;
using UnityEngine.InputSystem;

public class SceneControl_MoveTest : MonoBehaviour
{
    // - EXTRA CLASS -
    [System.Serializable]
    public struct CharacterData
    {
        public MoveBase_Controller linkController;
        public Move_Control linkCharacter;
        public bool useCameraView;
        public Vector3 cameraTargetOffset;

        public void SetControl(bool active, PlayerInput playerInput)
        {
            if (active == false)
            {
                linkController.SetControl(false);
                linkController.RemoveInput();
                return;
            }

            linkController.SetCharacter(linkCharacter);
            linkController.ConfigureInput(playerInput);
            linkController.SetCameraOptions(useCameraView);
            linkController.SetControl(true);
        }
    }


    // - EVENTS -
    void Event_OnCharacterTabChange(int index)
    {
        int prev = _currentCharacter;

        _currentCharacter = index;

        ChangeCharacter(prev, _currentCharacter);
    }


    // - VARIABLES -
    [SerializeField] private CharacterData[] _characterList;
    private int _currentCharacter = 0;

    [Header("- Camera -")]
    [SerializeField] private Camera_Controller _cameraController;

    [Header("- Interface -")]
    [SerializeField] private Panel_Tabs _characterTabs;
    [SerializeField] private Panel_Stats _characterStats;
    [SerializeField] private RectTransform _characterMeters;

    // Movement
    private InputAction _inputPrevCharacter;
    private InputAction _inputNextCharacter;

    private const string INPUT_PREVIOUS = "Tab_Left";
    private const string INPUT_NEXT = "Tab_Right";


    // - UNITY -
    void Awake()
    {
        // Set timeScale to normal
        Time.timeScale = 1;

        if (_characterTabs != null)
            _characterTabs.OnTabSelected += Event_OnCharacterTabChange;
    }
    private void Start()
    {
        PlayerInput playerInput = Input_Manager.Instance.GetPlayer(0);

        ChangeCharacter(-1, 0);

        SetInputs(playerInput);
        _cameraController.ConfigureInput(playerInput);
    }
    private void OnDestroy()
    {
        RemoveInputs();
    }


    // - INPUTS -
    void SetInputs(PlayerInput playerInput)
    {
        _inputPrevCharacter = playerInput.actions[INPUT_PREVIOUS];
        _inputNextCharacter = playerInput.actions[INPUT_NEXT];

        _inputPrevCharacter.performed += Input_PreviousCharacter;
        _inputNextCharacter.performed += Input_NextCharacter;
    }
    void RemoveInputs()
    {
        _inputPrevCharacter.performed -= Input_PreviousCharacter;
        _inputNextCharacter.performed -= Input_NextCharacter;
    }

    void Input_PreviousCharacter(InputAction.CallbackContext cnt)
    {
        if (_characterTabs != null)
        {
               _characterTabs.Action_Previous();
               return;
        }

        int prev = _currentCharacter;
        _currentCharacter--;
        if (_currentCharacter < 0)
            _currentCharacter = _characterList.Length - 1;
        ChangeCharacter(prev, _currentCharacter);     
    }
    void Input_NextCharacter(InputAction.CallbackContext cnt)
    {
        if (_characterTabs != null)
        {
            _characterTabs.Action_Next();
            return;
        }

        int prev = _currentCharacter;
        _currentCharacter++;
        if (_currentCharacter >= _characterList.Length)
            _currentCharacter = 0;
        ChangeCharacter(prev, _currentCharacter);
    }


    // - PRIVATE -
    void ChangeCharacter(int previous, int next)
    {
        if (previous >= 0)
            _characterList[previous].SetControl(false, null);

        CharacterData tempChar = _characterList[next];

        _cameraController.SetTarget(tempChar.linkController, tempChar.linkCharacter.transform, tempChar.cameraTargetOffset);
       
        PlayerInput playerInput = Input_Manager.Instance.GetPlayer(0);

        tempChar.SetControl(true, playerInput);

        tempChar.linkCharacter.ShowMeters(_characterMeters);
        tempChar.linkCharacter.ShowStats(_characterStats);
    }
}