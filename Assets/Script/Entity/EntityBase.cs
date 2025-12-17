using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EntityBase : NetworkBehaviour, IPoolObj, ITakeDamage
{
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private NavMeshAgent agent;

    private Transform cachedTransform;
    private ITakeDamage currentTarget;
    private Transform currentTargetTransform;

    private float lastAttackTime;
    private float lastTargetValidityCheckTime;

    private bool isTrackingPlayer;

    private const float TARGET_VALIDITY_CHECK_INTERVAL = 10f;

    [SerializeField] HealthBar healthBar;

    public NetworkVariable<float> currentHP = new NetworkVariable<float>(
100,
NetworkVariableReadPermission.Everyone,
NetworkVariableWritePermission.Server
);

    public NetworkVariable<float> maxHP = new NetworkVariable<float>(
100,
NetworkVariableReadPermission.Everyone,
NetworkVariableWritePermission.Server
);

    private void Reset()
    {
        if (null == GetComponent<NetworkTransform>())
        {
            transform.AddComponent<NetworkTransform>();
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            transform.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        agent = GetComponent<NavMeshAgent>();
        if (null == agent)
        {
            agent = transform.AddComponent<NavMeshAgent>();
        }
        healthBar = GetComponentInChildren<HealthBar>();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        cachedTransform = transform;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        agent.speed = speed;

        healthBar.UpdateHealthPercent(1);

        currentHP.OnValueChanged += OnHealthChanged;

        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(currentHP.Value / maxHP.Value);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        float currentTime = Time.time;

        // 타겟 유효성 체크 (10초마다 or 타겟 없을 때)
        if (currentTarget == null || currentTime - lastTargetValidityCheckTime >= TARGET_VALIDITY_CHECK_INTERVAL)
        {
            lastTargetValidityCheckTime = currentTime;
            FindTarget();
        }

        // 타겟이 없으면 리턴
        if (currentTarget == null || currentTargetTransform == null)
            return;

        // 타겟과의 거리 계산
        float distanceToTarget = Vector3.Distance(cachedTransform.position, currentTargetTransform.position);

        if (distanceToTarget <= attackRange)
        {
            // 공격 범위 안 - 멈추고 공격
            agent.isStopped = true;
            Attack();
        }
        else
        {
            // 공격 범위 밖 - 추적
            agent.isStopped = false;
            agent.SetDestination(currentTargetTransform.position);
        }
    }

    private void FindTarget()
    {
        // 1. Player 추적 시도
        if (TryFindPlayer())
        {
            isTrackingPlayer = true;
            return;
        }

        // 2. Player 추적 불가 - Wall 타겟 찾기
        isTrackingPlayer = false;
        FindNearestWall();
    }

    private bool TryFindPlayer()
    {
        List<Player> playerList = PlayerManager.Instance.GetPlayers();
        if (playerList.Count == 0)
            return false;

        float nearestDistance = float.MaxValue;
        Player nearestReachablePlayer = null;

        // 모든 플레이어를 체크해서 "갈 수 있는" 가장 가까운 플레이어 찾기
        foreach (Player player in playerList)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(cachedTransform.position, player.transform.position);

            // 이미 찾은 플레이어보다 멀면 스킵 (최적화)
            if (distance >= nearestDistance)
                continue;

            // NavMesh 경로 체크
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(player.transform.position, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    // 갈 수 있고, 더 가까움!
                    nearestDistance = distance;
                    nearestReachablePlayer = player;
                }
            }
        }

        // 갈 수 있는 플레이어를 찾았으면
        if (nearestReachablePlayer != null)
        {
            currentTarget = nearestReachablePlayer.GetComponent<ITakeDamage>();
            currentTargetTransform = nearestReachablePlayer.transform;
            return true;
        }

        return false;
    }

    private void FindNearestWall()
    {
        List<WallBuilding> buildings = BattleGameMode.Instance.GetWalls();
        if (buildings.Count == 0)
        {
            currentTarget = null;
            currentTargetTransform = null;
            return;
        }

        float nearestDistance = float.MaxValue;
        WallBuilding nearestWall = null;

        foreach (WallBuilding wall in buildings)
        {
            if (wall == null) continue;

            float distance = Vector3.Distance(cachedTransform.position, wall.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestWall = wall;
            }
        }

        if (nearestWall != null)
        {
            currentTarget = nearestWall.GetComponent<ITakeDamage>();
            currentTargetTransform = nearestWall.transform;
        }
        else
        {
            currentTarget = null;
            currentTargetTransform = null;
        }
    }

    private void Attack()
    {
        if (!IsServer)
        {
            return;
        }

        float currentTime = Time.time;
        if (currentTime - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = currentTime;

        if (currentTarget != null)
        {
            bool isDead = currentTarget.TakeDamage(attackDamage);

            if (isDead)
            {
                // 타겟 사망 - 새 타겟 찾기
                currentTarget = null;
                currentTargetTransform = null;
                lastTargetValidityCheckTime = 0; // 즉시 재탐색
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Player 추적 중이면 충돌 무시
        if (isTrackingPlayer)
            return;

        // ITakeDamage 체크
        ITakeDamage damageableTarget = collision.gameObject.GetComponent<ITakeDamage>();
        if (damageableTarget == null)
            return;

        // EntityBase 체크
        EntityBase entity = collision.gameObject.GetComponent<EntityBase>();
        if (entity != null)
            return;

        // 충돌 대상을 새 타겟으로 설정
        currentTarget = damageableTarget;
        currentTargetTransform = collision.transform;
        lastTargetValidityCheckTime = Time.time;
    }

    public void OnPush()
    {
        currentTarget = null;
        currentTargetTransform = null;
        isTrackingPlayer = false;
    }

    public void OnPop()
    {
        lastAttackTime = 0;
        lastTargetValidityCheckTime = 0;
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthPercent(newValue / maxHP.Value);
        }
        if (newValue <= 0)
        {
            PoolManager.Instance.Push(gameObject);
        }
    }


    public virtual bool TakeDamage(float _Amount)
    {
        if (!IsServer) return false;  // 서버에서만 처리

        currentHP.Value -= _Amount;


        if (currentHP.Value < 0)
        {
            currentHP.Value = 0;
            return true;
        }

        else
        {
            return false;
        }
    }

    public virtual bool HealHP(float _Amount)
    {
        if (!IsServer) return false;  // 서버에서만 처리

        currentHP.Value += _Amount;

        if (currentHP.Value > maxHP.Value)
        {
            currentHP.Value = maxHP.Value;
        }

        return true;
    }
}