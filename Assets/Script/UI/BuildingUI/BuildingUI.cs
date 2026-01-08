using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : UIBase
{
    [SerializeField] float hidePosX = 180f;
    [SerializeField] Button DestroyButton;

    RectTransform rt;

    Vector2 showPos = Vector2.zero;

    private void Reset()
    {
        hidePosX = GetComponent<RectTransform>().rect.width;
        DestroyButton = GetComponentInChildren<Button>();
    }

    override protected void Awake()
    {
        base.Awake();
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(hidePosX, 0f);

        DestroyButton.onClick.AddListener(DestroyBuilding);
    }

    protected override void Start()
    {
        base.Start();
        Canvas.ForceUpdateCanvases();
    }

    public void Show()
    {
        rt.DOKill();

        rt.DOAnchorPos(showPos, 0.35f)
          .SetEase(Ease.OutCubic);
    }

    public void Hide()
    {
        rt.DOKill();

        rt.DOAnchorPos(new Vector2(hidePosX, 0f), 0.35f)
          .SetEase(Ease.InCubic);
    }

    public void DestroyBuilding()
    {
        BuildingBase selectedBuilding = BuildingClickController.Instance.selectedBuilding;

        NetworkObjectReference reference = new NetworkObjectReference(selectedBuilding.NetworkObject);
        BuildingManager.Instance.RemoveBuildingServerRpc(reference, GameInstance.Instance.GetPlayerID());


    }
}
