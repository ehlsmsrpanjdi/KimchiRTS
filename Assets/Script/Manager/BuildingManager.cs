// BuildingManager.cs
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 서버에서만 쓰는 관리 리스트
    private readonly Dictionary<ulong, Dictionary<BuildingType, List<GameObject>>> BuildingList = new();

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlaceBuildingServerRpc(string buildingName, Vector3 worldPos, Vector2Int currentGridPos,
        int width, int height, ulong playerID)
    {
        if (!IsServer) return;

        // 1) 서버 풀에서 꺼냄 (Spawn/SetActive 안 함)
        GameObject buildingGo = PoolManager.Instance.Pop(buildingName, worldPos);
        if (buildingGo == null) return;

        buildingGo.transform.rotation = Quaternion.identity;

        // 2) NetworkObject 필수
        var netObj = buildingGo.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError($"[BuildingManager] NetworkObject missing on {buildingName}");
            return;
        }

        // 3) BuildingBase 세팅
        BuildingBase buildingBase = buildingGo.GetComponent<BuildingBase>();
        if (buildingBase == null)
        {
            Debug.LogError("[BuildingManager] BuildingBase Component is Null");
            return;
        }

        buildingBase.SetGridInfo(currentGridPos, width, height);
        buildingBase.BuildingOwnerId.Value = playerID;

        // 4) 네트워크 Spawn (모든 클라 자동 생성/활성)
        //    이미 Spawn된 객체면 예외/경고 날 수 있으니 방어
        if (!netObj.IsSpawned)
            netObj.Spawn();

        // 5) 서버 그리드 등록 (서버 권한 상태)
        GridArea.Instance.PlaceBuilding(buildingGo, currentGridPos.x, currentGridPos.y, width, height);

        // 6) 서버 관리 리스트 등록
        if (!BuildingList.TryGetValue(playerID, out var typeDict))
        {
            typeDict = new Dictionary<BuildingType, List<GameObject>>();
            BuildingList[playerID] = typeDict;
        }

        var typeKey = buildingBase.BuildingType;
        if (!typeDict.TryGetValue(typeKey, out var list))
        {
            list = new List<GameObject>();
            typeDict[typeKey] = list;
        }

        list.Add(buildingGo);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RemoveBuildingServerRpc(NetworkObjectReference buildingRef, ulong playerID)
    {
        if (!IsServer) return;

        if (!buildingRef.TryGet(out NetworkObject netObj)) return;

        GameObject buildingGo = netObj.gameObject;

        BuildingBase buildingBase = buildingGo.GetComponent<BuildingBase>();
        if (buildingBase == null) return;

        // 1) 서버 관리 리스트에서 제거
        if (BuildingList.TryGetValue(playerID, out var typeDict) &&
            typeDict.TryGetValue(buildingBase.BuildingType, out var list))
        {
            list.Remove(buildingGo);
        }

        // 2) 네트워크 Despawn (모든 클라에서 자동으로 비활성/제거됨)
        //    Destroy=false → 메모리에 남음 (풀링 가능)
        if (netObj.IsSpawned)
            netObj.Despawn(false);

        // 3) 서버 풀에 반환 (SetActive 금지)
        PoolManager.Instance.Push(buildingGo);
    }
}
