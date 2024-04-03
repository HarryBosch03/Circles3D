using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

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
                    StartHost();
                }
                if (GUILayout.Button("Start Client"))
                {
                    StartClient();
                }
            }
        }

        private void Update()
        {
            if (!InstanceFinder.NetworkManager.IsOffline) return;

            if (Keyboard.current.hKey.wasPressedThisFrame) StartHost();
            if (Keyboard.current.cKey.wasPressedThisFrame) StartClient();
        }

        private void StartHost()
        {
            InstanceFinder.ServerManager.StartConnection();
            StartClient();
        }

        private void StartClient()
        {
            InstanceFinder.ClientManager.StartConnection();
        }
    }
}