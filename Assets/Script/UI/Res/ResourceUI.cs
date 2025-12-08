using TMPro;
using UnityEngine;

public class ResourceUI : UIBase
{
    [SerializeField] TextMeshProUGUI resText;
    private void Reset()
    {
        resText = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
    }

    public override void NetWorkBinding()
    {
        PlayerResource res = GameInstance.Instance.GetPlayer().playerResource;
        res.OnChangeResource += UpdateResourceUI;

        UpdateResourceUI(res.Resources.Value);
    }

    void UpdateResourceUI(int newAmount)
    {
        resText.text = newAmount.ToString();
    }
}
