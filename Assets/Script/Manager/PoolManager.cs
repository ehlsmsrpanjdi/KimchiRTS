using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();

    // ==================== PUSH ====================
    public void Push(GameObject obj)
    {
        LogHelper.Log($"Push 시작: {obj.name}");

        var netObj = obj.GetComponent<NetworkObject>();
        LogHelper.Log($"NetworkObject 존재: {netObj != null}");

        if (netObj != null)
        {
            LogHelper.Log($"IsServer: {NetworkManager.Singleton.IsServer}");

            if (!NetworkManager.Singleton.IsServer)
            {
                LogHelper.Log("클라이언트라서 리턴");
                return;
            }

            LogHelper.Log($"IsSpawned: {netObj.IsSpawned}");

            if (netObj.IsSpawned)
            {
                LogHelper.Log("Despawn 호출");
                netObj.Despawn(false);
                LogHelper.Log("Despawn 완료");
            }

            return; // ✅ 여기서 리턴되는지 확인
        }

        // NetworkObject 없으면 → 로컬 Pool (UI 등)
        string key = obj.name;
        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();

        obj.GetComponent<IPoolObj>()?.OnPush();
        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }

    // ==================== POP ====================
    public GameObject Pop(string key, Vector3 position)
    {
        GameObject prefab = AssetManager.Instance.GetByName(key);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {key}");
            return null;
        }

        var netObj = prefab.GetComponent<NetworkObject>();

        // NetworkObject 있으면 → 서버에서만 Spawn
        if (netObj != null)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning($"Client tried to spawn NetworkObject: {key}");
                return null;
            }

            GameObject obj = GameObject.Instantiate(prefab, position, Quaternion.identity);
            obj.name = key;

            var networkObject = obj.GetComponent<NetworkObject>();
            networkObject.Spawn(true); // Handler가 자동으로 Pool에서 꺼냄

            obj.GetComponent<IPoolObj>()?.OnPop();
            return obj;
        }

        // NetworkObject 없으면 → 로컬 Pool
        if (!poolDictionary.ContainsKey(key))
            poolDictionary[key] = new Queue<GameObject>();

        GameObject localObj;

        if (poolDictionary[key].Count > 0)
        {
            localObj = poolDictionary[key].Dequeue();
            localObj.transform.position = position;
            localObj.SetActive(true);
        }
        else
        {
            localObj = GameObject.Instantiate(prefab, position, Quaternion.identity);
            localObj.name = key;
        }

        localObj.GetComponent<IPoolObj>()?.OnPop();
        return localObj;
    }

    public GameObject Pop(string key)
    {
        return Pop(key, Vector3.zero);
    }
}

public interface IPoolObj
{
    void OnPush();
    void OnPop();
}