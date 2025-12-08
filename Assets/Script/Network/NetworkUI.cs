using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{

    private void OnGUI()
    {
        float w = 200f, h = 40f;
        /*float x = 10f, */float y = 10f;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(w, y, w, h), "Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUI.Button(new Rect(w, y + h + 10, w, h), "Client"))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUI.Button(new Rect(w, y + 2 * (h + 10), w, h), "Server"))
            {
                NetworkManager.Singleton.StartServer();
            }

        }
    }

}
