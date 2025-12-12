public class BuildSystemUI : UIBase
{
    public BuildingElementUI buildingElement;
    public BuildingSystemToggleUI buildingSystemToggleUI;

    private void Reset()
    {
        buildingElement = this.TryFindChild("BuildingElementUI").GetComponent<BuildingElementUI>();
        buildingSystemToggleUI = this.TryFindChild("BuildingSystemToggle").GetComponent<BuildingSystemToggleUI>();
    }


}
