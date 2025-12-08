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
    }
}
