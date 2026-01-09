// PoolManager.cs
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

    // ✅ 기존 타입 그대로 유지
    private readonly Dictionary<string, Queue<GameObject>> poolDictionary
        = new Dictionary<string, Queue<GameObject>>();

    // 서버 전용 가드 (에디터에서 실수 잡기)
    private static void AssertServer()
    {
        if (NetworkManager.Singleton != null)
            Debug.Assert(NetworkManager.Singleton.IsServer);
    }

    /// <summary>
    /// ✅ 서버 전용
    /// ✅ 이 함수는 "꺼내기만" 합니다. (Spawn/SetActive 절대 안 함)
    /// </summary>
    public GameObject Pop(string key, Vector3 position)
    {
        AssertServer();

        if (!poolDictionary.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            poolDictionary[key] = q;
        }

        GameObject obj = null;

        // 큐에서 살아있는 객체를 찾음 (혹시 null 들어간 경우 대비)
        while (q.Count > 0 && obj == null)
        {
            obj = q.Dequeue();
        }

        // 없으면 새로 생성 (서버에서만)
        if (obj == null)
        {
            GameObject prefab = AssetManager.Instance.GetByName(key);
            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Prefab not found: {key}");
                return null;
            }

            obj = Object.Instantiate(prefab);
            obj.name = key;
        }

        obj.transform.position = position;

        // 서버 초기화 훅 (서버만 호출)
        obj.GetComponent<IPoolObj>()?.OnPop();

        return obj;
    }

    /// <summary>
    /// ✅ 서버 전용
    /// ✅ 이 함수는 "보관만" 합니다. (SetActive 절대 안 함)
    /// ⚠️ 호출 전에 반드시 netObj.Despawn(false)가 선행되어야 합니다.
    /// </summary>
    public void Push(GameObject obj)
    {
        AssertServer();

        if (obj == null) return;

        obj.SetActive(false);

        string key = obj.name;

        if (!poolDictionary.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            poolDictionary[key] = q;
        }

        // 서버 정리 훅 (서버만 호출)
        obj.GetComponent<IPoolObj>()?.OnPush();

        q.Enqueue(obj);
    }
}

public interface IPoolObj
{
    // ⚠️ 서버에서만 호출된다고 가정하는 훅
    void OnPush();
    void OnPop();
}
