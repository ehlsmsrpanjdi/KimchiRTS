using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Sprites")]
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private SpriteRenderer currentHealthSprite;

    private static Camera mainCamera;
    private Quaternion lastCameraRotation;

    private float currentFillAmount = 1f;

    Vector3 startSize;
    Vector3 startPosition;

    private void Reset()
    {
        if (backgroundSprite == null)
        {
            // Background 자동 생성 및 설정
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform);
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localScale = Vector3.one;

            backgroundSprite = bgObj.AddComponent<SpriteRenderer>();
            backgroundSprite.sprite = CreateSprite(Color.black);
            backgroundSprite.sortingOrder = 0;
        }

        if (currentHealthSprite == null)
        {
            // CurrentHealth 자동 생성 및 설정
            GameObject healthObj = new GameObject("CurrentHealth");
            healthObj.transform.SetParent(transform);
            healthObj.transform.localPosition = Vector3.zero;
            healthObj.transform.localScale = Vector3.one;

            currentHealthSprite = healthObj.AddComponent<SpriteRenderer>();
            currentHealthSprite.sprite = CreateSprite(Color.green);
            currentHealthSprite.sortingOrder = 1;
        }
    }

    private Sprite CreateSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        // Pivot을 Left로 설정
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f), 1);
    }

    private void Awake()
    {
        startSize = currentHealthSprite.transform.localScale;
        startPosition = currentHealthSprite.transform.localPosition;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = FollowCamera.Instance.Camera;


        currentFillAmount = 1f;
    }



    /// <summary>
    /// 체력 퍼센트 업데이트 (0~1) - 즉시 반영
    /// </summary>
    public void UpdateHealthPercent(float percent)
    {
        currentFillAmount = Mathf.Clamp01(percent);
        UpdateHealthBarVisual();
        UpdateHealthBarColor();
    }

    private void LateUpdate()
    {
        // 화면 밖이면 업데이트 스킵 (최적화)
        if (!IsVisibleFromCamera())
            return;

        // Billboard - 카메라 회전 변경 시에만 업데이트 (최적화)
        if (mainCamera != null && mainCamera.transform.rotation != lastCameraRotation)
        {
            transform.rotation = mainCamera.transform.rotation;
            lastCameraRotation = mainCamera.transform.rotation;
        }
    }

    private void UpdateHealthBarVisual()
    {
        if (currentHealthSprite != null)
        {
            currentHealthSprite.transform.localScale = new Vector3(currentFillAmount, startSize.y , startSize.z);
        }
    }

    private void UpdateHealthBarColor()
    {
        if (currentHealthSprite != null)
        {
            if (currentFillAmount > 0.5f)
                currentHealthSprite.color = Color.green;
            else if (currentFillAmount > 0.25f)
                currentHealthSprite.color = Color.yellow;
            else
                currentHealthSprite.color = Color.red;
        }
    }

    /// <summary>
    /// 화면에 보이는지 체크 (Frustum Culling)
    /// </summary>
    private bool IsVisibleFromCamera()
    {
        if (mainCamera == null || backgroundSprite == null)
            return true;

        // Renderer의 isVisible 사용 (Unity가 자동으로 Frustum Culling)
        return backgroundSprite.isVisible;
    }
}