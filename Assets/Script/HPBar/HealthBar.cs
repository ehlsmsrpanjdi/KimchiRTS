using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Sprites")]
    [SerializeField] private SpriteRenderer backgroundSprite;
    [SerializeField] private SpriteRenderer currentHealthSprite;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 5f; // 체력바 부드럽게 변하는 속도

    private static Camera mainCamera;
    private static Quaternion lastCameraRotation;

    [SerializeField] private Transform ownerTransform;
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;
    private bool isHealthChanging = false;

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
        // 체력바 크기 설정
        transform.localScale = new Vector3(2f, 0.2f, 1f);
    }

    private Sprite CreateSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        // Pivot을 Left로 설정
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f), 1);
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = FollowCamera.Instance.Camera;

        currentFillAmount = 1f;
        targetFillAmount = 1f;
    }

    /// <summary>
    /// Owner 설정
    /// </summary>
    public void SetOwner(Transform owner)
    {
        ownerTransform = owner;
    }

    /// <summary>
    /// 체력 퍼센트 업데이트 (0~1)
    /// </summary>
    public void UpdateHealthPercent(float percent)
    {
        targetFillAmount = Mathf.Clamp01(percent);
        isHealthChanging = true;
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

        // 체력 변화 있을 때만 업데이트 (최적화)
        if (isHealthChanging)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            UpdateHealthBarVisual();

            if (Mathf.Abs(currentFillAmount - targetFillAmount) < 0.001f)
            {
                currentFillAmount = targetFillAmount;
                isHealthChanging = false;
            }
        }
    }

    private void UpdateHealthBarVisual()
    {
        if (currentHealthSprite != null)
        {
            // Scale의 X축으로 체력바 길이 조절
            Vector3 scale = currentHealthSprite.transform.localScale;
            scale.x = currentFillAmount;
            currentHealthSprite.transform.localScale = scale;

            // Position 조정 (Pivot이 Left이므로)
            Vector3 pos = currentHealthSprite.transform.localPosition;
            pos.x = (currentFillAmount - 1f) * 0.5f;
            currentHealthSprite.transform.localPosition = pos;
        }
    }

    private void UpdateHealthBarColor()
    {
        if (currentHealthSprite != null)
        {
            if (targetFillAmount > 0.5f)
                currentHealthSprite.color = Color.green;
            else if (targetFillAmount > 0.25f)
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
