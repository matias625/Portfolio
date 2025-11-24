using UnityEngine;

[System.Serializable]
public class Param_Icon
{
    public string parameter;
    public Sprite icon;

    // - CONSTRUCTOR -
    public Param_Icon(Param_Icon _copy)
    {
        parameter = _copy.parameter;
        icon = _copy.icon;
    }
    public Param_Icon(string _param, Sprite _icon)
    {
        parameter = _param;
        icon = _icon;
    }
    public Param_Icon() { }
}