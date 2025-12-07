using System;

public class GameInstance
{
    static GameInstance instance;
    public static GameInstance Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameInstance();
            }
            return instance;
        }
    }

    public Action<int> OnChangeResource;

    int resource = 0;

    public void ChangeResource(int delta)
    {
        resource += delta;
        OnChangeResource?.Invoke(resource);
    }
}
