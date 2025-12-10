using System.Collections.Generic;

public class PlayerManager
{
    static PlayerManager instance;

    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }
    }

    List<Player> playerList = new List<Player>();

    public void AddPlayer(Player _Player)
    {
        playerList.Add(_Player);
        LogHelper.Log("프레이어젒고");
    }

    public void RemovePlayer(Player _Player)
    {
        playerList.Remove(_Player);
    }

    public List<Player> GetPlayers()
    {
        return playerList;
    }
}
