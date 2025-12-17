using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : UIBase
{
    [SerializeField] ResourceUI resourceUI;
    [SerializeField] TextMeshProUGUI playerUI;
    [SerializeField] TextMeshProUGUI WaveUI;
    [SerializeField] TextMeshProUGUI NextWaveTimeUI;
    [SerializeField] TextMeshProUGUI DifficultyUI;
    [SerializeField] Button WaveStartBtn;


    private void Reset()
    {
        resourceUI = this.TryFindChild("ResourceUI").GetComponent<ResourceUI>();
        playerUI = this.TryFindChild("PlayerUI").GetComponentInChildren<TextMeshProUGUI>();
        WaveUI = this.TryFindChild("WaveUI").GetComponentInChildren<TextMeshProUGUI>();
        NextWaveTimeUI = this.TryFindChild("NextWaveTimeUI").GetComponentInChildren<TextMeshProUGUI>();
        DifficultyUI = this.TryFindChild("DifficultyUI").GetComponentInChildren<TextMeshProUGUI>();
        WaveStartBtn = GetComponentInChildren<Button>();
    }

    protected override void Awake()
    {
        base.Awake();

        WaveStartBtn.onClick.AddListener(() => MonsterSpawnManager.Instance.OnSpanwer());
    }

    protected override void Start()
    {
        GameInstance.Instance.onPlayerAdd += SetPlayerUI;
    }

    public void SetPlayerUI()
    {
        int Count = GameInstance.Instance.GetPlayerCount();
        playerUI.text = $"{Count} / 4";
    }

    public void SetWaveUI(int Count)
    {
        WaveUI.text = $"Wave : {Count} / 20";
    }

    public void SetRemainWaveTime(string _str)
    {
        NextWaveTimeUI.text = _str;
    }

    void resNet()
    {
        PlayerResource res = GameInstance.Instance.GetPlayer().playerResource;
        res.OnChangeResource += resourceUI.UpdateResourceUI;
        resourceUI.UpdateResourceUI(res.Resources.Value);
    }


    public override void NetWorkBinding()
    {
        base.NetWorkBinding();

        resNet();
    }
}
