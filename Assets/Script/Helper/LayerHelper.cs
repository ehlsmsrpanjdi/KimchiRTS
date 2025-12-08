using UnityEngine;

public class LayerHelper
{
    static LayerHelper instance;
    public static LayerHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LayerHelper();
            }
            return instance;
        }
    }

    public const string GridLayer = "Grid";

    public int GetLayerToInt(string _str)
    {
        return 1 << LayerMask.NameToLayer(_str);
    }
}
