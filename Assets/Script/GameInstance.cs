using System.Collections.Generic;

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

    private ulong playerID = 0;


    public Dictionary<ulong, Player> playerDic = new Dictionary<ulong, Player>();

    public void SetPlayerID(ulong _ID)
    {
        playerID = _ID;
        LogHelper.Log($"plyerID = " + _ID);
    }

    public void AddPlayer(ulong playerID, Player _player)
    {
        playerDic.Add(playerID, _player);
    }

    public ulong GetPlayerID()
    {
        return playerID;
    }

    public Player GetPlayer()
    {
        return playerDic[playerID];
    }

    public Player GetPlayer(ulong _playerID)
    {
        return playerDic[_playerID];
    }

}
