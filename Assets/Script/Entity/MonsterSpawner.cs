using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    float currentTime;
    float SpawnTime = 5;


    private void Awake()
    {
        MonsterSpawnManager.Instance.AddSpawner(this);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime < SpawnTime)
        {
            return;
        }

        currentTime -= SpawnTime;
        if (NetworkManager.Singleton.IsServer == true)
        {
            GameObject obj = PoolManager.Instance.Pop("EntityBase", transform.position);

            //obj.transform.position = transform.position;

            var networkTransform = obj.GetComponent<NetworkTransform>();
            if (networkTransform != null)
            {
                networkTransform.Teleport(
                    transform.position,
                    transform.rotation,
                    transform.localScale
                );
            }
        }
    }
}
