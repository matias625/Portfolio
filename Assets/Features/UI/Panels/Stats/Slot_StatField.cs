using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Slot_StatField : MonoBehaviour
{
    // - EVENTS -
    UnityAction<float> OnValueChange;

    void Event_OnEndEdit(string value)
    {
        if (float.TryParse(value, out var val))
        {
            OnValueChange.Invoke(val);
            return;
        }
        Debug.Log($"Cant convert text to float: {value}");
    }


    // - VARIABLES -
    [SerializeField] TextMeshProUGUI _txtName;
    [SerializeField] TMP_InputField _fieldValue;

    public void Configure(string statName, string defaultValue, UnityAction<float> onValueChange, bool intNumbers = false)
    {
        OnValueChange = onValueChange;

        _txtName.text = statName;

        _fieldValue.text = defaultValue;

        if (intNumbers)
            _fieldValue.contentType = TMP_InputField.ContentType.IntegerNumber;
        else
            _fieldValue.contentType = TMP_InputField.ContentType.DecimalNumber;
    }


    // - UNITY -
    private void Awake()
    {
        _fieldValue.onEndEdit.AddListener(Event_OnEndEdit);
    }
}