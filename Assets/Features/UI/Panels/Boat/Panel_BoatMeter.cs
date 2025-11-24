using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_BoatMeter : MonoBehaviour
{
    // - EVENTS -
    void Event_OnRudderRotation(float porcent)
    {
        float remap = (porcent + 1f) / 2f;

        _scrollRudder.value = remap;
    }
    void Event_OnDesiredSpeedChange(float current, float max)
    {
        float porcent = current / max;
        _sliderDesiredSpeed.value = porcent;

        float rounded = (float)Math.Round(current, 2);
        _txtDesiredSpeed.text = $"{rounded}/{max}";
    }
    void Event_OnBoatSpeedChange(float current, float max)
    {
        float porcent = current / max;
        _sliderCurrentSpeed.value = porcent;

        float rounded = (float)Math.Round(current, 2);
        _txtCurrentSpeed.text = $"{rounded}/{max}";
    }


    // - VARIABLES -
    [Header("- Desired Speed -")]
    [SerializeField] private Slider _sliderDesiredSpeed;
    [SerializeField] private TextMeshProUGUI _txtDesiredSpeed;
    [Header("- Current Speed -")]
    [SerializeField] private Slider _sliderCurrentSpeed;   
    [SerializeField] private TextMeshProUGUI _txtCurrentSpeed;

    [Header("- Rudder Rotation -")]
    [SerializeField] private Scrollbar _scrollRudder;

    Move_Boat _accesBoat;


    // - PUBLIC -
    public void AssignBoat(Move_Boat boat)
    {
        RemoveBoat();

        _accesBoat = boat;

        _accesBoat.OnRudderRotation += Event_OnRudderRotation;
        _accesBoat.OnDesiredSpeedChange += Event_OnDesiredSpeedChange;
        _accesBoat.OnBoatSpeedChange += Event_OnBoatSpeedChange;
    }
    void RemoveBoat()
    {
        if (_accesBoat == null) return;

        _accesBoat.OnRudderRotation -= Event_OnRudderRotation;
        _accesBoat.OnDesiredSpeedChange -= Event_OnDesiredSpeedChange;
        _accesBoat.OnBoatSpeedChange -= Event_OnBoatSpeedChange;

        _accesBoat = null;
    }

    // - UNITY -
    private void OnDestroy()
    {
        RemoveBoat();
    }
}