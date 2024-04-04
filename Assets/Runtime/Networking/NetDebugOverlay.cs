using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Runtime.Networking
{
    public class NetDebugOverlay : MonoBehaviour
    {
        public string sessionName = "UntitledRoom";
        public NetworkRunner netRunnerPrefab;

        private NetworkRunner netRunnerInstance;

        private void Update()
        {
            if (!netRunnerInstance)
            {
                var kb = Keyboard.current;
                if (kb.hKey.wasPressedThisFrame) StartRunner(GameMode.Host);
                if (kb.cKey.wasPressedThisFrame) StartRunner(GameMode.Client);
            }
        }

        private void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(0, 0, 150, Screen.height)))
            {
                if (!netRunnerInstance)
                {
                    sessionName = GUILayout.TextField(sessionName);
                    if (GUILayout.Button("Host")) StartRunner(GameMode.Host);
                    if (GUILayout.Button("Join")) StartRunner(GameMode.Client);
                }
            }
        }

        private async void StartRunner(GameMode gamemode)
        {
            netRunnerInstance = Instantiate(netRunnerPrefab);
            netRunnerInstance.ProvideInput = true;

            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }

            await netRunnerInstance.StartGame(new StartGameArgs()
            {
                SessionName = sessionName,
                GameMode = gamemode,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }
    }
}