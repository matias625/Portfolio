using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Slot_StatToggle : MonoBehaviour
{
    // - VARIABLES -
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] Toggle _toggleValue;


    // - PUBLIC -
    public void Configure(string statName, bool value, UnityAction<bool> onValueChange)
    {
        _toggleValue.onValueChanged.RemoveAllListeners();

        _txtName.text = statName;

        _toggleValue.isOn = value;
        _toggleValue.onValueChanged.AddListener(onValueChange);
    }
}