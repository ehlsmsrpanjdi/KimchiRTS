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

    Dictionary<ulong, Dictionary<BuildingType, List<GameObject>>> BuildingList = new();

    //   이 메세지는 서버한테 보낸다,  이 함수는 서버가 누구한테 다시 보낼거다
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlaceBuildingServerRpc(string buildingName, Vector3 worldPos, Vector2Int currentGridPos, int width, int height, ulong playerID)
    {
        var buildingToSpawn = PoolManager.Instance.Pop(buildingName, worldPos);

        buildingToSpawn.transform.rotation = Quaternion.identity;

        //buildingToSpawn.GetComponent<NetworkObject>().Spawn();

        // BuildingBase 컴포넌트 가져오기 또는 추가
        BuildingBase buildingBase = buildingToSpawn.GetComponent<BuildingBase>();
        if (buildingBase == null)
        {
            LogHelper.LogError("BuildingBase Component is Null");
        }

        // 빌딩 정보 설정
        buildingBase.SetGridInfo(currentGridPos, width, height);

        buildingBase.BuildingOwnerId.Value = playerID;

        // 그리드에 등록
        GridArea.Instance.PlaceBuilding(buildingToSpawn, currentGridPos.x, currentGridPos.y, width, height);


        if (!BuildingList.TryGetValue(playerID, out var typeDict))
        {
            typeDict = new Dictionary<BuildingType, List<GameObject>>();
            BuildingList[playerID] = typeDict;
        }

        if (!typeDict.TryGetValue(buildingBase.BuildingType, out var list))
        {
            list = new List<GameObject>();
            typeDict[buildingBase.BuildingType] = list;
        }

        list.Add(buildingToSpawn);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RemoveBuildingServerRpc(NetworkObjectReference buildingRef, ulong playerID)
    {
        if (!IsServer) return;

        if (buildingRef.TryGet(out NetworkObject networkObject))
        {
            GameObject building = networkObject.gameObject;
            BuildingBase buildingBase = building.GetComponent<BuildingBase>();
            if (buildingBase == null) return;

            // ✅ 1. 리스트에서 제거
            if (BuildingList.ContainsKey(playerID) &&
                BuildingList[playerID].ContainsKey(buildingBase.BuildingType))
            {
                BuildingList[playerID][buildingBase.BuildingType].Remove(building);
            }

            // ✅ 2. Grid에서 제거 (BuildingBase 내부 메서드 활용)
            Vector2Int gridPos = buildingBase.gridPosition; // gridPosition을 public으로 변경 필요
            int sizeX = buildingBase.sizeX; // sizeX를 public으로 변경 필요
            int sizeY = buildingBase.sizeY; // sizeY를 public으로 변경 필요

            GridArea.Instance.RemoveBuilding(gridPos.x, gridPos.y, sizeX, sizeY);

            // ✅ 3. Pool로 반환 (이때 OnPush 호출됨)
            PoolManager.Instance.Push(building);
        }
    }
}
