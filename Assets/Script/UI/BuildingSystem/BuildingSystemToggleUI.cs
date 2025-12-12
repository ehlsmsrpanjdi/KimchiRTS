using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingSystemToggleUI : MonoBehaviour, IPointerClickHandler
{
    public Image toggleImg;

    public RectTransform targetPanel;  // 움직일 Panel

    float startHeight;

    private void Reset()
    {
        toggleImg = GetComponent<Image>();
    }

    private void Awake()
    {
        if (targetPanel == null)
        {
            targetPanel = this.TryFindChild("BuildingsystemBackGround").GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        startHeight = targetPanel.anchoredPosition.y;
    }

    bool isOpen = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        if (isOpen == false)
        {
            isOpen = true;
            // 토글 버튼은 원래 startHeight 위치로 이동
            GetComponent<RectTransform>()
                .DOAnchorPosY(-startHeight, 0.25f)
                .SetEase(Ease.OutCubic);

            // 슬라이드 패널은 y = 0 으로 이동
            targetPanel
                .DOAnchorPosY(0f, 0.25f)
                .SetEase(Ease.OutCubic);
        }
        else
        {
            isOpen = false;
            // 토글 버튼은 원래 startHeight 위치로 이동
            GetComponent<RectTransform>()
                .DOAnchorPosY(0, 0.25f)
                .SetEase(Ease.OutCubic);

            // 슬라이드 패널은 y = 0 으로 이동
            targetPanel
                .DOAnchorPosY(startHeight, 0.25f)
                .SetEase(Ease.OutCubic);
        }
    }

    public void OnToggle()
    {
        if (isOpen == true)
        {
            return;
        }

        isOpen = true;
        // 토글 버튼은 원래 startHeight 위치로 이동
        GetComponent<RectTransform>()
            .DOAnchorPosY(-startHeight, 0.25f)
            .SetEase(Ease.OutCubic);

        // 슬라이드 패널은 y = 0 으로 이동
        targetPanel
            .DOAnchorPosY(0f, 0.25f)
            .SetEase(Ease.OutCubic);
    }

    public void OffToggle()
    {
        if (isOpen == false)
        {
            return;
        }
        isOpen = false;
        // 토글 버튼은 원래 startHeight 위치로 이동
        GetComponent<RectTransform>()
            .DOAnchorPosY(0, 0.25f)
            .SetEase(Ease.OutCubic);

        // 슬라이드 패널은 y = 0 으로 이동
        targetPanel
            .DOAnchorPosY(startHeight, 0.25f)
            .SetEase(Ease.OutCubic);
    }
}
