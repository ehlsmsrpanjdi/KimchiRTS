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

    private void Awake()
    {
        startSize = currentHealthSprite.transform.localScale;

        // SpriteRenderer의 실제 Bounds 사용 (가장 정확)
        Bounds bgBounds = backgroundSprite.bounds;

        // 현재 중심점 계산
        Vector3 currentCenter = bgBounds.center;
        Vector3 parentPos = transform.position;

        // 부모 기준으로 얼마나 이동해야 하는지 계산
        Vector3 offsetToCenter = transform.InverseTransformPoint(currentCenter);

        // 중심으로 이동
        backgroundSprite.transform.localPosition = -offsetToCenter;
        currentHealthSprite.transform.localPosition = -offsetToCenter;

        startPosition = currentHealthSprite.transform.localPosition;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = FollowCamera.Instance.Camera;
        currentFillAmount = 1f;
    }

    public void UpdateHealthPercent(float percent)
    {
        currentFillAmount = Mathf.Clamp01(percent);
        UpdateHealthBarVisual();
        UpdateHealthBarColor();
    }

    private void LateUpdate()
    {
        if (!IsVisibleFromCamera())
            return;

        if (mainCamera != null)
        {
            // 매 프레임 월드 회전을 카메라 방향으로 고정
            transform.rotation = mainCamera.transform.rotation;
            lastCameraRotation = mainCamera.transform.rotation;
        }
    }

    private void UpdateHealthBarVisual()
    {
        if (currentHealthSprite != null)
        {
            // Scale만 변경 (Pivot이 왼쪽이므로 왼쪽 기준으로 줄어듦 - 정상 동작)
            currentHealthSprite.transform.localScale = new Vector3(
                currentFillAmount,
                startSize.y,
                startSize.z
            );
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

    private bool IsVisibleFromCamera()
    {
        if (mainCamera == null || backgroundSprite == null)
            return true;
        return backgroundSprite.isVisible;
    }
}