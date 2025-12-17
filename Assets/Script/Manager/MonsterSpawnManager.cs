using System.Collections.Generic;

public class MonsterSpawnManager
{
    static MonsterSpawnManager instance;

    List<MonsterSpawner> monsters = new List<MonsterSpawner>();

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

    public void AddSpawner(MonsterSpawner spawner)
    {
        monsters.Add(spawner);
    }

    public void OnSpanwer()
    {
        foreach (var spawner in monsters)
        {
            spawner.gameObject.SetActive(true);
        }

    }
}
