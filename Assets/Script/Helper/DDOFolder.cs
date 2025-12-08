using UnityEngine;

public class DDOFolder : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
