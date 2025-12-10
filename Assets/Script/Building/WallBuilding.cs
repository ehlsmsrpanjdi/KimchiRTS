public class WallBuilding : BuildingBase
{
    protected override void Awake()
    {

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
