using Unity.Netcode;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    //   이 메세지는 서버한테 보낸다,  이 함수는 서버가 누구한테 다시 보낼거다
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlaceBuildingServerRpc(string buildingName, Vector3 worldPos, Vector2Int currentGridPos, int width, int height, ulong playerID)
    {
        var buildingToSpawn = PoolManager.Instance.Pop(buildingName);

        buildingToSpawn.transform.position = worldPos;
        buildingToSpawn.transform.rotation = Quaternion.identity;

        buildingToSpawn.GetComponent<NetworkObject>().Spawn();

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
    }
}
