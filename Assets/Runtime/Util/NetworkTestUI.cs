using FishNet;
using UnityEngine;

namespace Runtime.Util
{
    public class NetworkTestUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (!InstanceFinder.NetworkManager.IsOffline) return;
            
            using (new GUILayout.AreaScope(new Rect(0f, 0f, 150f, Screen.height)))
            {
                if (GUILayout.Button("Start Host"))
                {
                    InstanceFinder.ServerManager.StartConnection();
                    InstanceFinder.ClientManager.StartConnection();
                }
                if (GUILayout.Button("Start Client"))
                {
                    InstanceFinder.ClientManager.StartConnection();
                }
            }
        }
    }
}