using UnityEngine;

public class WallBuilding : BuildingBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        HealHP(1 * Time.deltaTime);
    }

    public override void OnPop()
    {
        BattleGameMode.Instance.AddWall(this);
    }

    public override void OnPush()
    {
        BattleGameMode.Instance.RemoveWall(this);
    }
}
