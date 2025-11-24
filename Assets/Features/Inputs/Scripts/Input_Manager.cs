using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Input_Manager : MonoBehaviour
{
    // - EVENTS -
    public event Action<InputDevice, InputDeviceChange> OnDeviceChange;
    public event Action<PlayerInput> OnPlayerJoined;
    public event Action<PlayerInput> OnPlayerLeft;

    void OnDeviceChangeHandler(InputDevice _device, InputDeviceChange _change)
    {
        ObtainDevices(); // Refresh devices list

        OnDeviceChange?.Invoke(_device, _change);
    }
    void OnPlayerJoinedHandler(PlayerInput _input)
    {
        _myInputs.Add(_input);

        _input.gameObject.name = "Player_" + _input.playerIndex;
        _input.transform.SetParent(transform); // Set parent to Input_Manager for organization

        OnPlayerJoined?.Invoke(_input);
    }
    void OnPlayerLeftHandler(PlayerInput _input)
    {
        _myInputs.Remove(_input);

        OnPlayerLeft?.Invoke(_input);
    }

    // - VARIABLES -
    [Header("- Input -")]
    [SerializeField] PlayerInputManager _inputManager = null;
    List<PlayerInput> _myInputs = new List<PlayerInput>();
    List<InputDevice[]> _devices = new List<InputDevice[]>();

    [Header("- Icons -")]
    [SerializeField] Asset_Input_Icons _linkIcons;

    // - GET -
    public static Input_Manager Instance { get; private set; }

    public bool GetPlayersAtMaxCapacity => _myInputs.Count >= _inputManager.maxPlayerCount;
    public int GetPlayersCount => _myInputs.Count;
    public PlayerInput GetPlayer(int _index)
    {
        if (_index < 0 || _index >= _myInputs.Count) return null;
        return _myInputs[_index];
    }
    public bool CheckPlayersHaveDevices
    {
        get
        {
            foreach (PlayerInput input in _myInputs)
            {
                if (input.devices.Count == 0) return false;
            }
            return true;
        }
    }


    // - UNITY -
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Input_Manager already initialized.");
            return;
        }
        Debug.Log("Input_Manager Initialize");
        Instance = this;
        transform.parent = null; // Detach from any parent to avoid hierarchy issues
        DontDestroyOnLoad(gameObject);

        // Subscribe to PlayerInputManager events
        _inputManager.onPlayerJoined += OnPlayerJoinedHandler;
        _inputManager.onPlayerLeft += OnPlayerLeftHandler;
        InputSystem.onDeviceChange += OnDeviceChangeHandler;
        // Obtain initial devices
        ObtainDevices();
    }


    // - PUBLIC -
    /// <summary>
    /// Enables or disables the ability for players to join the game.
    /// </summary>
    /// <param name="_enabled">A value indicating whether player joining should be enabled.  <see langword="true"/> to enable player joining;
    /// otherwise, <see langword="false"/> to disable it.</param>
    public void EnableJoinPlayer(bool _enabled)
    {
        if (_enabled)
        {
            Debug.Log("Enable Joining Player");
            _inputManager.EnableJoining();
        }
        else
        {
            Debug.Log("Disable Joining Player");
            _inputManager.DisableJoining();
        }
    }
    public PlayerInput CreatePlayer(string _scheme, InputDevice[] _devices)
    {
        return _inputManager.JoinPlayer(-1, -1, _scheme, _devices);
    }


    #region - PUBLIC : Player -

    public void ChangePlayerMap(int _player, string _mapName, bool _enableUI = true)
    {
        if (_player < 0 || _player >= _myInputs.Count) return;
        PlayerInput input = _myInputs[_player];
        if (input == null) return;
        // Change the action map
        input.SwitchCurrentActionMap(_mapName);
        // Enable the action map
        if (!_mapName.Equals("UI") && _enableUI)
            input.actions.FindActionMap("UI").Enable();
    }
    public void ChangeAllPlayerMaps(string _mapName, bool _enableUI = true)
    {
        foreach (PlayerInput input in _myInputs)
        {
            if (input == null) continue;
            // Change the action map
            input.SwitchCurrentActionMap(_mapName);
            // Enable the action map
            if (!_mapName.Equals("UI") && _enableUI)
                input.actions.FindActionMap("UI").Enable();
        }
    }

    public void ChangePlayerDevice(int _player, InputDevice[] _devices)
    {
        if (_player < 0 || _player >= _myInputs.Count) return;
        PlayerInput input = _myInputs[_player];
        if (input == null) return;
        // Change the device
        input.neverAutoSwitchControlSchemes = true; // Prevent automatic switching
        input.SwitchCurrentControlScheme(_devices);
    }
    public void ChangePlayerDevice(int _player, int _deviceIndex)
    {
        if (_player < 0 || _player >= _myInputs.Count) return;
        PlayerInput input = _myInputs[_player];
        if (input == null) return;

        if (_deviceIndex < 0 && _deviceIndex >= _devices.Count) return;
        input.neverAutoSwitchControlSchemes = true; // Prevent automatic switching
        input.SwitchCurrentControlScheme(_devices[_deviceIndex]);
    }

    #endregion

    #region - PUBLIC : Devices -

    // - GET -
    public InputDevice[] GetDeviceAt(int _index)
    {
        if (_index >= 0 && _index < _devices.Count)
            return _devices[_index];
        return null;
    }
    public int GetDeviceIndex(int _player)
    {
        return GetDeviceIndex(_myInputs[_player]);
    }
    public int GetDeviceIndex(PlayerInput _input)
    {
        if (_input.devices.Count == 0) return -1;

        InputDevice currentDevice = _input.devices[0];
        for (int i = 0; i < _devices.Count; i++)
        {
            if (_devices[i][0] == currentDevice)
                return i;
        }
        return -1;
    }
    public string GetDeviceName(int _index)
    {
        if (_index >= 0 && _index < _devices.Count)
            return _devices[_index][0].displayName;

        return "Not Exist Index : " + _index;
    }
    /// <summary>
    /// Gets a list of dropdown options representing the available input devices.
    /// </summary>
    /// <remarks>Each option in the returned list corresponds to the first device in each array of input
    /// devices. The icon for each device is retrieved using the <c>GetDeviceIcon</c> method.</remarks>
    public List<TMP_Dropdown.OptionData> GetDeviceOptionDataList
    {
        get
        {
            List<TMP_Dropdown.OptionData> result = new List<TMP_Dropdown.OptionData>();

            foreach (InputDevice[] devs in _devices)
            {
                result.Add(new TMPro.TMP_Dropdown.OptionData(devs[0].displayName,
                    GetDeviceIcon(devs[0]), Color.white));
            }

            return result;
        }
    }

    // - PUBLIC -
    /// <summary>
    /// Populates a collection of input device groups, including keyboards, mice, and supported controllers.
    /// </summary>
    /// <remarks>This method identifies and groups input devices into logical collections based on their type.
    /// It includes the current keyboard and mouse (if available) as one group, and each supported  controller (e.g.,
    /// joysticks, gamepads, or other controllers) as separate groups. The resulting  collection can be used to manage
    /// or process input devices in a structured way.</remarks>
    public void ObtainDevices()
    {
        _devices = new List<InputDevice[]>();

        if (Keyboard.current != null)
        {
            _devices.Add(new InputDevice[] { Keyboard.current, Mouse.current });
        }

        for (int a = 0; a < InputSystem.devices.Count; a++)
        {
            string name = InputSystem.devices[a].GetType().Name;

            if (name.Contains("Joystick") ||
                name.Contains("Gamepad") ||
                name.Contains("Controller"))
            {
                // Add Device
                _devices.Add(new InputDevice[] { InputSystem.devices[a] });
            }
        }
    }

    #endregion

    #region - PUBLIC : Icons -

    // - GET : Device -
    public string GetDeviceName(PlayerInput _player)
    {
        if (_player.devices.Count > 0)
            return _player.devices[0].displayName;

        return "Not Found";
    }
    /// <summary>
    /// Retrieves the icon associated with the specified input device for a given player.
    /// </summary>
    /// <param name="_device">The input device for which the icon is being retrieved.</param>
    /// <param name="_player">The index of the player whose input configuration is used. Must be a valid player index.</param>
    /// <returns>A <see cref="Sprite"/> representing the icon for the specified input device and player. Returns <see
    /// langword="null"/> if no icon is available for the given device or player.</returns>
    public Sprite GetDeviceIcon(InputDevice _device, int _player = 0)
    {
        return _linkIcons.GetDeviceIcon(_device, _myInputs[_player]);
    }
    /// <summary>
    /// Retrieves the icon representing the input device used by the specified player.
    /// </summary>
    /// <param name="_player">The player whose input device icon is to be retrieved. Cannot be null.</param>
    /// <returns>A <see cref="Sprite"/> representing the icon of the player's input device.  Returns <see langword="null"/> if no
    /// icon is available for the specified device.</returns>
    public Sprite GetDeviceIcon(PlayerInput _player)
    {
        return _linkIcons.GetDeviceIcon(_player);
    }
    /// <summary>
    /// Retrieves the icon representing the device used by the specified player.
    /// </summary>
    /// <param name="_player">The index of the player whose device icon is to be retrieved. Must be a valid index within the range of
    /// available players.</param>
    /// <returns>A <see cref="Sprite"/> representing the player's device icon.</returns>
    public Sprite GetDeviceIcon(int _player)
    {
        return _linkIcons.GetDeviceIcon(_myInputs[_player]);
    }

    // - GET : Button -
    public Sprite GetButtonIcon(PlayerInput _player, string _type)
    {
        if (_linkIcons == null)
        {
            Debug.LogWarning("Input_Managers not have asigned linkIcons(Asset_Input_Icons)");
            return null;
        }
        return _linkIcons.GetButtonIcon(_player, _type);
    }
    public Sprite GetButtonIcon(int _player, string _type)
    {
        return _linkIcons.GetButtonIcon(_myInputs[_player], _type);
    }

    #endregion
}