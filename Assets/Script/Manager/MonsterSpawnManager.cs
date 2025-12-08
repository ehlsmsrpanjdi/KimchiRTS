public class MonsterSpawnManager
{
    static MonsterSpawnManager instance;
    public static MonsterSpawnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MonsterSpawnManager();
            }
            return instance;
        }
    }

}
