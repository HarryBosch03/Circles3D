using System;
using Fusion;
using Fusion.Photon.Realtime;
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
            var pad = 10;
            using (new GUILayout.AreaScope(new Rect(pad, pad, 150, Screen.height - pad * 2)))
            {
                if (!netRunnerInstance)
                {
                    sessionName = GUILayout.TextField(sessionName);
                    if (GUILayout.Button("Host")) StartRunner(GameMode.Host);
                    if (GUILayout.Button("Join")) StartRunner(GameMode.Client);
                }
                else
                {
                    DrawPingDisplay("You", netRunnerInstance.LocalPlayer);
                    GUILayout.Space(8);
                    foreach (var player in netRunnerInstance.ActivePlayers)
                    {
                        if (player == netRunnerInstance.LocalPlayer) continue;
                        DrawPingDisplay("You", player);
                    }
                }
            }
        }

        private void DrawPingDisplay(string name, PlayerRef player)
        {
            if (!player.IsRealPlayer) return;
            GUILayout.Label($"{name} | {(int)(netRunnerInstance.GetPlayerRtt(player) * 1000f),4:N0}ms");
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
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                IsOpen = true,
                IsVisible = true,
            });
        }
    }
}