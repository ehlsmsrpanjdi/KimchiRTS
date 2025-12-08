using Unity.Netcode;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{

    private void Update()
    {
        if (Time.frameCount % 300 == 0 && NetworkManager.Singleton.IsServer == true)
        {
            GameObject obj = PoolManager.Instance.Pop("EntityBase");

            obj.transform.position = transform.position;
            LogHelper.Log("스폰");
        }
    }
}
