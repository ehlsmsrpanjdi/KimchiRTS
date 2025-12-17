using UnityEngine;

public class BattleBuilding : BuildingBase
{
    [SerializeField] private BattleRangeDetector detector;
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private float checkInterval = 1f; // 체크 간격

    private float lastAttackTime = 0f;
    private float lastCheckTime = 0f;

    protected override void Reset()
    {
        base.Reset();
        detector = GetComponentInChildren<BattleRangeDetector>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        // 1초마다만 체크 (나머지 프레임은 스킵)
        if (Time.time - lastCheckTime < checkInterval)
            return;

        lastCheckTime = Time.time;

        // 공격 쿨타임 체크
        if (Time.time - lastAttackTime >= attackInterval)
        {
            EntityBase target = detector.GetClosestEntity(transform.position);

            if (target != null)
            {
                Attack(target);
                lastAttackTime = Time.time;
            }
        }
    }

    private void Attack(EntityBase target)
    {
        GameObject baseBullet = PoolManager.Instance.Pop(ResourceString.TestBullet, transform.position);
        BulletBase bullet = baseBullet.GetComponent<BulletBase>();
        bullet.Initialize(target);
    }
}