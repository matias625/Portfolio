using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tool_Input_Icon : MonoBehaviour
{
    // - VARIABLES -
    [SerializeField] Input_Icon[] listIcons;

    // - UNITY -
    private void Start()
    {
        if (Input_Manager.Instance != null)
            Input_Manager.Instance.GetPlayer(0).controlsChangedEvent.AddListener(Update_InputIcons);

        Update_InputIcons(Input_Manager.Instance.GetPlayer(0));
    }
    private void OnDestroy()
    {
        if (Input_Manager.Instance != null)
            Input_Manager.Instance.GetPlayer(0).controlsChangedEvent.RemoveListener(Update_InputIcons);
    }

    // - PRIVATE - 
    void Update_InputIcons(UnityEngine.InputSystem.PlayerInput _player)
    {
        foreach(Input_Icon icon in listIcons)
        {
            icon.imgIcon.sprite = Input_Manager.Instance.GetButtonIcon(_player, icon.numberInput.ToString());
        }
    }

    // - EXTRA CLASSES -
    [System.Serializable]
    public class Input_Icon
    {
        public Image imgIcon;
        public int numberInput;
    }
}
