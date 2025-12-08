using UnityEngine;

public class DebugManager : MonoBehaviour
{

    public static DebugManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            Instance = this;
        }
    }

    public GameObject Ghost;

    public void CreateGhost()
    {
        PoolManager.Instance.Pop("CubeGhost");
    }

}
