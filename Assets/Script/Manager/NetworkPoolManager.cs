using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPoolManager : MonoBehaviour
{
    public static NetworkPoolManager Instance;

    // Prefab별 Pool
    private Dictionary<string, Queue<NetworkObject>> pools = new();

    // Handler 등록 여부
    private HashSet<string> registeredPrefabs = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += RegisterAllHandlers;
        }
    }

    void RegisterAllHandlers()
    {
        RegisterPrefabsByLabel(ResourceString.LabelBuilding);
        RegisterPrefabsByLabel(ResourceString.LabelBullet);
        RegisterPrefabsByLabel(ResourceString.LabelEntity);
    }

    void RegisterPrefabsByLabel(string label)
    {
        var prefabs = AssetManager.Instance.GetPrefabsByLabel(label);

        foreach (var prefab in prefabs)
        {
            // ✅ NetworkObject 있는 애들만 등록
            var netObj = prefab.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                RegisterPrefab(prefab.name);
            }
        }
    }

    void RegisterPrefab(string prefabName)
    {
        if (registeredPrefabs.Contains(prefabName))
            return;

        GameObject prefab = AssetManager.Instance.GetByName(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {prefabName}");
            return;
        }

        NetworkObject netObj = prefab.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError($"NetworkObject component missing: {prefabName}");
            return;
        }

        var handler = new PooledPrefabHandler(prefabName, this);
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, handler);

        pools[prefabName] = new Queue<NetworkObject>();
        registeredPrefabs.Add(prefabName);

        LogHelper.Log($"Registered pool handler: {prefabName}");
    }

    public NetworkObject GetFromPool(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefabName))
            pools[prefabName] = new Queue<NetworkObject>();

        NetworkObject obj;

        if (pools[prefabName].Count > 0)
        {
            obj = pools[prefabName].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
        }
        else
        {
            GameObject prefab = AssetManager.Instance.GetByName(prefabName);
            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = prefabName;
            obj = instance.GetComponent<NetworkObject>();
        }

        return obj;
    }

    public void ReturnToPool(string prefabName, NetworkObject obj)
    {
        if (!pools.ContainsKey(prefabName))
            pools[prefabName] = new Queue<NetworkObject>();

        obj.gameObject.SetActive(false);
        pools[prefabName].Enqueue(obj);
    }
}

// INetworkPrefabInstanceHandler 구현
class PooledPrefabHandler : INetworkPrefabInstanceHandler
{
    string prefabName;
    NetworkPoolManager poolManager;

    public PooledPrefabHandler(string name, NetworkPoolManager manager)
    {
        prefabName = name;
        poolManager = manager;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        LogHelper.Log($"🔵 Handler.Instantiate 호출: {prefabName}"); // 이거 뜨는지
        return poolManager.GetFromPool(prefabName, position, rotation);
    }

    public void Destroy(NetworkObject networkObject)
    {
        LogHelper.Log($"🔴 Handler.Destroy 호출: {prefabName}"); // 이거 뜨는지
        poolManager.ReturnToPool(prefabName, networkObject);
    }
}