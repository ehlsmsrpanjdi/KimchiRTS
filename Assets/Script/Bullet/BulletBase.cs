using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class BulletBase : NetworkBehaviour, IPoolObj
{
    [Header("Bullet Settings")]
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected float lifeTime = 5f; // n초 후 자동 소멸
    [SerializeField] protected int damage = 10;

    public EntityBase target;
    private float spawnTime;
    private bool isMoving = false;

    private void Reset()
    {
        NetworkObject obj = GetComponent<NetworkObject>();
        if (obj == null)
        {
            transform.AddComponent<NetworkObject>();
            transform.AddComponent<NetworkTransform>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spawnTime = Time.time;
        isMoving = true;
    }

    private void Update()
    {
        if (!IsServer || !isMoving)
            return;

        // 생존 시간 체크
        if (Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        MoveFunction();
    }

    public virtual void MoveFunction()
    {
        if (target == null || target.gameObject == null)
        {
            // 타겟이 없으면 직진
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
            return;
        }

        // 타겟을 향해 이동
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 타겟 방향으로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        EntityBase entity = other.GetComponent<EntityBase>();
        if (entity != null && entity == target)
        {
            entity.TakeDamage(damage);
            ReturnToPool();
        }
    }

    /// <summary>
    /// 총알 초기화 및 발사
    /// </summary>
    public void Initialize(EntityBase targetEntity, int damageAmount = 10)
    {
        target = targetEntity;
        damage = damageAmount;
        spawnTime = Time.time;
        isMoving = true;

        // 초기 방향 설정
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void ReturnToPool()
    {
        isMoving = false;
        target = null;

        // 풀로 반환 로직 (예시)
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    public void OnPush()
    {
        isMoving = false;
        target = null;
    }

    public void OnPop()
    {
        spawnTime = Time.time;
        isMoving = true;
    }
}