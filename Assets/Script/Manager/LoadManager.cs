using System.Threading.Tasks;


public class LoadManager
{
    static LoadManager instance;
    public static LoadManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LoadManager();
            }
            return instance;
        }
    }

    public async Task LoadTemp()
    {
        await AssetManager.Instance.LoadByLabelAsync("Entity");
        await AssetManager.Instance.LoadByLabelAsync("Building");
        await AssetManager.Instance.LoadByLabelAsync("UI");
    }
}


public static class ResourceString
{
    public const string LabelUI = "UI";
    public const string LabelEntity = "Entity";
    public const string LabelBuilding = "Building";


    #region UI

    public const string ResBuildingName = "Building";

    #endregion

}