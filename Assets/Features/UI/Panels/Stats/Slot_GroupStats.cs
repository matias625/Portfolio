using UnityEngine;
using TMPro;

public class Slot_GroupStats : MonoBehaviour
{
    // - VARIABLES -
    [SerializeField] TextMeshProUGUI _txtGroupName;

    [SerializeField] RectTransform _container;
    [SerializeField] Slot_StatSlider _pfStatSlider;
    [SerializeField] Slot_StatToggle _pfStatToggle;
    [SerializeField] Slot_StatField _pfStatField;


    // - PUBLIC -
    public void Configure(string name)
    {
        _txtGroupName.text = name;
    }

    public Slot_StatSlider CreateSlider(string statName)
    {
        GameObject go = Instantiate(_pfStatSlider.gameObject, _container);
        go.name = statName;
        return go.GetComponent<Slot_StatSlider>();
    }
    public Slot_StatToggle CreateToggle(string statName)
    {
        GameObject go = Instantiate(_pfStatToggle.gameObject, _container);
        go.name = statName;
        return go.GetComponent<Slot_StatToggle>();
    }
    public Slot_StatField CreateField(string statName)
    {
        GameObject go = Instantiate(_pfStatField.gameObject, _container);
        go.name = statName;
        return go.GetComponent<Slot_StatField>();
    }
}