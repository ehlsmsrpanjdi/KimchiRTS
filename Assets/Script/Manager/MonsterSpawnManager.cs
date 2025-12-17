using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterSpawnManager : NetworkBehaviour
{
    static MonsterSpawnManager instance;

    List<MonsterSpawner> monsters = new List<MonsterSpawner>();

    public static MonsterSpawnManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private NetworkVariable<float> currentWave = new NetworkVariable<float>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (currentWave.Value <= 0)
        {
            return;
        }
        currentWave.Value -= Time.deltaTime;
    }


    public float GetCurrentTimerValue()
    {
        return currentWave.Value;
    }


    public void AddSpawner(MonsterSpawner spawner)
    {
        monsters.Add(spawner);
    }

    public void OnSpanwer()
    {
        currentWave.Value = 300;
        foreach (var spawner in monsters)
        {
            spawner.gameObject.SetActive(true);
        }
        currentWave.OnValueChanged += UIManager.Instance.GetUI<GameUI>().SetRemainWaveTime;
    }
}
