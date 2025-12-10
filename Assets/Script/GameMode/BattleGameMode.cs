using System.Collections.Generic;
using UnityEngine;

public class BattleGameMode : MonoBehaviour
{
    static BattleGameMode instance;

    public static BattleGameMode Instance
    {
        get
        {
            return instance;
        }
    }

    public bool IsLoadEnd { get; private set; } = false;
    private void Awake()
    {
        IsLoadEnd = false;
        instance = this;
        TempLoad();
    }

    public async void TempLoad()
    {
        IsLoadEnd = false;
        await LoadManager.Instance.LoadTemp();  // 이 줄이 핵심
        IsLoadEnd = true;
        LogHelper.Log("로드 완리요");
    }

    List<WallBuilding> wallbuildings = new List<WallBuilding>();

    public void AddWall(WallBuilding _building)
    {
        wallbuildings.Add(_building);
    }

    public void RemoveWall(WallBuilding building)
    {
        wallbuildings.Remove(building);
    }

    public List<WallBuilding> GetWalls()
    {
        return wallbuildings;
    }
}
