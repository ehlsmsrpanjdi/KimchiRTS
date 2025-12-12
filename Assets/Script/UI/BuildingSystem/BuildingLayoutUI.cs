using UnityEngine;
using UnityEngine.UI;

public class BuildingLayoutUI : MonoBehaviour
{
    [SerializeField] Button ResBtn;
    [SerializeField] Button BtlBtn;
    [SerializeField] Button UpgBtn;


    private void Reset()
    {
        ResBtn = this.TryFindChild("ResourceBtn").GetComponent<Button>();
        BtlBtn = this.TryFindChild("BattleBtn").GetComponent<Button>();
        UpgBtn = this.TryFindChild("UpgradeBtn").GetComponent<Button>();
    }

    private void Awake()
    {
        ResBtn.onClick.AddListener(OnResBtn);
        BtlBtn.onClick.AddListener(OnBtlBtn);
        UpgBtn.onClick.AddListener(OnUpgBtn);
    }

    void OnResBtn()
    {
        BuildingElementUI ui = UIManager.Instance.GetUI<BuildSystemUI>().buildingElement;
        ui.ResetElement();
        LogHelper.Log("res");
        ui.AddCard(2);
    }

    void OnBtlBtn()
    {
        BuildingElementUI ui = UIManager.Instance.GetUI<BuildSystemUI>().buildingElement;
        ui.ResetElement(); LogHelper.Log("btl");
        ui.AddCard(1);
    }
    void OnUpgBtn()
    {
        BuildingElementUI ui = UIManager.Instance.GetUI<BuildSystemUI>().buildingElement;
        ui.ResetElement(); LogHelper.Log("upg");
    }
}
