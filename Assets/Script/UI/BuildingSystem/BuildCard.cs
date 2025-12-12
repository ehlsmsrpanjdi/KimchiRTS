using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildCard : MonoBehaviour, IPoolObj
{
    [SerializeField] Button clickBtn;

    Action OnClickCard;

    private void Reset()
    {
        clickBtn = GetComponent<Button>();
    }

    private void Awake()
    {
        clickBtn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        OnClickCard?.Invoke();
        UIManager.Instance.GetUI<BuildSystemUI>().buildingSystemToggleUI.OffToggle();
    }

    public void InitCard(int _CardIndex)
    {
        OnClickCard = BuildingDataManager.Instance.GetFun(_CardIndex);
    }

    public void OnPush()
    {
        OnClickCard = null;
    }

    public void OnPop()
    {
        OnClickCard = null;
    }
}
