using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EntityBase : NetworkBehaviour, IPoolObj
{
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float attackRange = 2f;

    [SerializeField] private NavMeshAgent agent;
    private Transform cachedTransform;
    private Transform targetPlayer;

    List<Player> playerList;

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
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        cachedTransform = transform;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Server에서만 AI 동작
        if (!IsServer)
        {
            agent.enabled = false;
            this.enabled = false;
            return;
        }

        agent.speed = speed;
    }

    private void Update()
    {
        // Server에서만 실행
        if (!IsServer) return;

        // 타겟 갱신 (1초마다 또는 타겟 없을 때만)
        if (targetPlayer == null || Time.frameCount % 60 == 0)
        {
            FindNearestPlayer();
        }

        if (targetPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(cachedTransform.position, targetPlayer.position);

        if (distanceToPlayer <= attackRange)
        {
            // 공격 범위 안 - 멈추고 공격
            agent.isStopped = true;
            Attack();
        }
        else
        {
            // 공격 범위 밖 - 추적
            agent.isStopped = false;
            agent.SetDestination(targetPlayer.position);
        }
    }

    private void FindNearestPlayer()
    {
        playerList = PlayerManager.Instance.GetPlayers();

        if (playerList.Count == 0)
        {
            targetPlayer = null;
            return;
        }

        float nearestDistance = float.MaxValue;
        Transform nearest = null;

        foreach (Player player in playerList)
        {
            if (player == null)
            {
                return;
            }
            float distance = Vector3.Distance(cachedTransform.position, player.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = player.transform;
            }
        }

        targetPlayer = nearest;
    }

    private void Attack()
    {
        Debug.Log($"{gameObject.name} attacking {targetPlayer.name}!");
    }

    public void OnPush()
    {
    }

    public void OnPop()
    {
    }
}