using UnityEngine;

public class Panel_Stats : MonoBehaviour
{
    // - VARIABLES -
    [SerializeField] RectTransform _container;
    [SerializeField] Slot_GroupStats _pfGroupStats;


    // - PUBLIC -
    public void CleanAll()
    {
        Tool_Cleaner.Clean_Transform(_container);
    }

    public Slot_GroupStats CreateGroup(string groupName)
    {
        GameObject go = Instantiate(_pfGroupStats.gameObject, _container);
        go.name = groupName;
        Slot_GroupStats temp = go.GetComponent<Slot_GroupStats>();
        temp.Configure(groupName);
        return temp;
    }
}