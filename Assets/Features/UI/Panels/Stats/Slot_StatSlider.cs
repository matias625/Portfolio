using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class Slot_StatSlider : MonoBehaviour
{
    // - EVENTS -
    UnityAction<float> OnValueChange;

    void Event_OnValueChange(float value)
    {
        // If value is int
        if (value % 1 == 0)
        {
            OnValueChange?.Invoke(value);
            _txtValue.text = value.ToString();
            return;
        }

        float rounded = (float)Math.Round(value, 2);

        OnValueChange?.Invoke(rounded);

        _txtValue.text = rounded.ToString();
    }


    // - VARIABLES -
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] Slider _sliderValue;
    [SerializeField] TextMeshProUGUI _txtValue;


    // - PUBLIC -
    public void Configure(string statName, float value, float min, float max, UnityAction<float> onValueChange, bool intNumbers = false)
    {
        OnValueChange = onValueChange;

        _txtName.text = statName;

        _sliderValue.minValue = min;
        _sliderValue.maxValue = max;
        _sliderValue.value = value;
        _sliderValue.wholeNumbers = intNumbers;

        _txtValue.text = value.ToString();
    }


    // - UNITY -
    private void Awake()
    {
        _sliderValue.onValueChanged.AddListener(Event_OnValueChange);
    }
}