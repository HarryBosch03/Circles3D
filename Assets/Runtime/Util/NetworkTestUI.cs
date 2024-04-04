using System.Diagnostics;
using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Util
{
    public class NetworkTestUI : MonoBehaviour
    {
        public string address = "localhost";
        public ushort targetPort = 8888;
        
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
                    StartClient(address);
                }
                if (GUILayout.Button("Ninja"))
                {
                    StartNinja();
                }
            }
        }

        private void Update()
        {
            if (!InstanceFinder.NetworkManager.IsOffline) return;

            if (Keyboard.current.hKey.wasPressedThisFrame) StartHost();
            if (Keyboard.current.cKey.wasPressedThisFrame) StartClient(address);
            if (Keyboard.current.nKey.wasPressedThisFrame) StartNinja();
        }

        private void StartHost()
        {
            InstanceFinder.ServerManager.StartConnection(targetPort);
            StartClient("localhost");
        }

        private void StartClient(string address)
        {
            InstanceFinder.ClientManager.StartConnection(address, targetPort);
        }
        
        private void StartNinja()
        {
            Process.Start("https://www.twitch.tv/ninja");
        }
    }
}