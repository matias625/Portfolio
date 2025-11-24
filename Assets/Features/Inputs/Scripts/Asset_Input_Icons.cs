using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// NOTAS: mover para que esté en la misma carpeta que Input_Manager!

[CreateAssetMenu(fileName = "assetInputIcons", menuName = "GameAssets/Input/New Icons", order = 1)]
public class Asset_Input_Icons : ScriptableObject
{
    // - VARIABLES - 
    [SerializeField] private List<Input_Data> listInputs = new List<Input_Data>();

    [Header("- Errors -")]
    [SerializeField] Sprite iconMissing;

    // - PUBLIC -
    public Sprite GetDeviceIcon(InputDevice _device, PlayerInput _input)
    {
        // Obtain the control scheme name that supports the device
        string schemeName = "";
        foreach (var scheme in _input.actions.controlSchemes)
        {
            if (scheme.SupportsDevice(_device))
            {
                schemeName = scheme.name;
                break;
            }
        }
        // If no control scheme supports the device, return the missing icon
        if (schemeName == "")
        {
            Debug.LogWarning($"No control scheme found for device '{_device.displayName}'. Using missing icon.");
            return iconMissing;
        }
        // Return the device icon for the found control scheme
        return GetDeviceIcon(_input, schemeName);
    }
    public Sprite GetDeviceIcon(PlayerInput _input, string _scheme = "")
    {
        // If no scheme is provided, use the current control scheme of the PlayerInput
        string schemeName = _scheme == "" ? _input.currentControlScheme : _scheme;
        // Find the corresponding Input_Data based on the scheme name
        int index = listInputs.FindIndex(x => x.schemeName == schemeName);
        // If the scheme is not found, return the missing icon
        if (index < 0)
        {
            Debug.LogWarning($"Input scheme '{schemeName}' not found in InputIcons. Using missing icon.");
            return iconMissing;
        }
        // Return the device icon
        return listInputs[index].deviceIcon;
    }

    public Sprite GetButtonIcon(PlayerInput _input, string _type)
    {
        string schemeName = _input.currentControlScheme;
        // Find the corresponding Input_Data based on the scheme name
        int index = listInputs.FindIndex(x => x.schemeName == schemeName);
        // If the scheme is not found, return the missing icon
        if (index < 0)
        {
            Debug.LogWarning($"Input scheme '{schemeName}' not found in InputIcons. Using missing icon.");
            return iconMissing;
        }

        Sprite tempButton = listInputs[index].GetInput(_type);

        if (tempButton == null)
        {
            Debug.LogWarning($"Button '{_type}' not found in scheme '{schemeName}'. Using missing icon.");
            return iconMissing;
        }

        return tempButton;
    }


    // - EXTRA CLASSES -
    [System.Serializable]
    public class Input_Data
    {
        // - VARIABLES -
        public string schemeName;
        public Sprite deviceIcon;
        public List<Param_Icon> inputIcons = new List<Param_Icon>();

        public Sprite GetInput(string _name)
        {
            int index = inputIcons.FindIndex(x => x.parameter == _name);

            if (index < 0)
            {
                Debug.LogWarning($"Input '{_name}' not found in scheme '{schemeName}'. Using missing icon.");
                return null;
            }

            return inputIcons[index].icon;
        }

        // - CONSTRUCTOR -
        public Input_Data(string _schemeName, Sprite _deviceIcon, List<Param_Icon> _inputIcons)
        {
            schemeName = _schemeName;
            deviceIcon = _deviceIcon;
            inputIcons = _inputIcons;
        }
        public Input_Data() { }
    }
}