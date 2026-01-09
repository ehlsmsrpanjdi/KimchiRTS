using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolManager
{
    static PoolManager instance;
    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
                instance = new PoolManager();
            return instance;
        }
    }

    // string = prefab name
    private Dictionary<string, Queue<GameObject>> poolDictionary
        = new Dictionary<string, Queue<GameObject>>();

    // -----------------------------------------------------
    // PUSH
    // -----------------------------------------------------
    public void Push(GameObject obj)
    {
        string key = obj.name;

        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();

        // ✅ NetworkObject Despawn 먼저
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(false); // destroy하지 않고 Despawn
        }

        obj.GetComponent<IPoolObj>()?.OnPush();
        obj.SetActive(false);

        poolDictionary[key].Enqueue(obj);
    }

    // -----------------------------------------------------
    // POP
    // -----------------------------------------------------
    public GameObject Pop(string key, Vector3 _position)
    {
        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();

        GameObject obj = null;

        // Pool에서 꺼내기
        if (poolDictionary[key].Count > 0)
        {
            obj = poolDictionary[key].Dequeue();
            obj.SetActive(true);
            obj.transform.position = _position;
        }
        else
        {
            // 새로 생성
            GameObject prefab = AssetManager.Instance.GetByName(key);
            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Prefab not found: {key}");
                return null;
            }

            obj = GameObject.Instantiate(prefab);
            obj.name = key;
            obj.transform.position = _position;
        }

        // ✅ Spawn 처리
        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn(true);
        }

        obj.GetComponent<IPoolObj>()?.OnPop();
        return obj;
    }
    public GameObject Pop(string key)
    {
        // 풀에 이 key가 없다면 초기화
        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();

        // 풀에 객체가 없으면 새로 생성
        if (poolDictionary[key].Count == 0)
        {
            GameObject prefab = AssetManager.Instance.GetByName(key);

            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Prefab not found: {key}");
                return null;
            }

            GameObject newObj = GameObject.Instantiate(prefab);
            newObj.name = key; // 풀링 키 유지
            newObj.GetComponent<IPoolObj>()?.OnPop();

            var spawnedNetObj = newObj.GetComponent<NetworkObject>();
            if (spawnedNetObj != null && !spawnedNetObj.IsSpawned)
            {
                spawnedNetObj.Spawn(true);  // Server에서 Spawn
            }

            return newObj;
        }

        // Pool에서 꺼내기
        GameObject obj = null;
        while (obj == null)
        {
            obj = poolDictionary[key].Dequeue();
            if (poolDictionary[key].Count <= 0)
            {
                obj = MonoBehaviour.Instantiate(AssetManager.Instance.GetByName(key));
                break;
            }
        }



        obj.SetActive(true);

        var netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn(true);  // Server에서 Spawn
        }


        obj.GetComponent<IPoolObj>()?.OnPop();
        return obj;
    }

}

public interface IPoolObj
{
    void OnPush();
    void OnPop();
}
