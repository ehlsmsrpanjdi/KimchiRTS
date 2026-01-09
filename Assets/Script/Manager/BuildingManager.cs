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
        LogHelper.Log($"RemoveBuildingServerRpc 호출됨 - IsServer: {IsServer}");

        if (buildingRef.TryGet(out NetworkObject networkObject))
        {
            LogHelper.Log($"NetworkObject 찾음: {networkObject.name}");

            GameObject building = networkObject.gameObject;
            BuildingBase buildingBase = building.GetComponent<BuildingBase>();
            if (buildingBase == null)
            {
                LogHelper.LogError("BuildingBase null");
                return;
            }

            LogHelper.Log($"Grid 제거 전 - gridPos: {buildingBase.gridPosition}");
            GridArea.Instance.RemoveBuilding(buildingBase.gridPosition.x, buildingBase.gridPosition.y,
                                            buildingBase.sizeX, buildingBase.sizeY);

            LogHelper.Log("PoolManager.Push 호출");
            PoolManager.Instance.Push(building);
            LogHelper.Log("PoolManager.Push 완료");
        }
        else
        {
            LogHelper.LogError("NetworkObjectReference.TryGet 실패");
        }
    }
}
