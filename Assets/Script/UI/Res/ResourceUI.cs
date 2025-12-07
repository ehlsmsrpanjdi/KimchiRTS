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
        GameInstance.Instance.OnChangeResource += UpdateResourceUI;
    }

    void UpdateResourceUI(int newAmount)
    {
        resText.text = newAmount.ToString();
    }
}
