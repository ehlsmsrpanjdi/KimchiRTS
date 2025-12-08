using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{

    private void Update()
    {
        if (Time.frameCount % 60 == 0 && NetworkManager.Singleton.IsServer == true)
        {
            GameObject obj = PoolManager.Instance.Pop("EntityBase");

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

            LogHelper.Log("스폰");
        }
    }
}
