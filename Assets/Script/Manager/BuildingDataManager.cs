using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingDataManager
{
    static BuildingDataManager instance;
    public static BuildingDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BuildingDataManager();
                instance.Init();
            }
            return instance;
        }
    }

    Dictionary<int, Action> buildingDic = new Dictionary<int, Action>();

    public Action GetFun(int _index)
    {
        return buildingDic[_index];
    }

    void Init()
    {
        buildingDic.Add(1, Fun_1);
        buildingDic.Add(2, Fun_2);
        buildingDic.Add(3, Fun_3);
    }

    void Fun_1()
    {
        GameObject obj = AssetManager.Instance.Spawn("Battle");
    }

    void Fun_2()
    {
        GameObject obj = AssetManager.Instance.Spawn("Res");
    }

    void Fun_3()
    {
        GameObject obj = AssetManager.Instance.Spawn("Wall");
    }

    void CreateGhost(int _index)
    {
        PoolManager.Instance.Pop("WallGhost", Vector3.zero);
    }
}
