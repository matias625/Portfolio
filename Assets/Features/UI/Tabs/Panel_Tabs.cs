using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class Panel_Tabs : MonoBehaviour
{
    // - EVENTS -
    public UnityAction<int> OnTabSelected;

    void OnInputChange(UnityEngine.InputSystem.PlayerInput input)
    {
        _iconPrevious.sprite = Input_Manager.Instance.GetButtonIcon(input, "TabPrev");
        _iconNext.sprite = Input_Manager.Instance.GetButtonIcon(input, "TabNext");
    }

    void Event_OnTabSelected(int index, bool isOn)
    {
        if (!isOn) return;

        _tabSelected = index;
        OnTabSelected?.Invoke(index);
    }

    // - VARIABLES -
    [SerializeField] ToggleGroup _myGroup;
    [SerializeField] Image _iconPrevious;
    [SerializeField] Image _iconNext;

    [SerializeField] Toggle[] _tabsList;
    int _tabSelected = 0;


    // - GET -
    public int TabSelected => _tabSelected;


    // - UNITY -
    void Awake()
    {
        int tabNum = 0;
        foreach (Toggle tog in _tabsList)
        {
            tog.onValueChanged.RemoveAllListeners();
            int num = tabNum;
            tog.onValueChanged.AddListener(data => Event_OnTabSelected(num, data));

            tabNum++;
        }
    }

    private void OnEnable()
    {
        Action_SelectTab(0);

        //Input_Manager.Instance.GetPlayer(0).controlsChangedEvent.AddListener(OnInputChange);

        //OnInputChange(Input_Manager.Instance.GetPlayer(0));
    }
    private void OnDisable()
    {
        if (Input_Manager.Instance == null) return;

        Input_Manager.Instance.GetPlayer(0).controlsChangedEvent.RemoveListener(OnInputChange);
    }

    // - PUBLIC -
    public void Action_SelectTab(int index)
    {
        _tabsList[index].isOn = true;
    }
    public void Action_Previous()
    {
        _tabSelected--;

        if (_tabSelected < 0)
            _tabSelected = _tabsList.Length - 1;
        

        _tabsList[_tabSelected].isOn = true;
    }
    public void Action_Next()
    {
        _tabSelected++;

        if (_tabSelected >= _tabsList.Length)
            _tabSelected = 0;
        

        _tabsList[_tabSelected].isOn = true;
    }


    // - PRIVATE -
}
