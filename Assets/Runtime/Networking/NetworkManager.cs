using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Runtime.Networking
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public NetworkPrefabRef playerPrefab;
        public string sessionName = "NewGame";

        private Dictionary<PlayerRef, NetworkObject> connectedPlayers = new();
        private NetworkRunner runner;

        public static NetworkManager instance { get; private set; }

        private void OnEnable()
        {
            instance = this;
        }

        public async void StartGame(GameMode gameMode)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;

            gameObject.AddComponent<RunnerSimulatePhysics3D>();

            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }
            
            await runner.StartGame(new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = sessionName,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
        }

        private void Update()
        {
            if (runner == null)
            {
                if (Keyboard.current.hKey.wasPressedThisFrame) StartGame(GameMode.Host);
                if (Keyboard.current.cKey.wasPressedThisFrame) StartGame(GameMode.Client);
                if (Keyboard.current.nKey.wasPressedThisFrame) Process.Start("https://www.twitch.tv/ninja");
            }
        }

        private void OnGUI()
        {
            if (runner == null)
            {
                sessionName = GUI.TextField(new Rect(0, 0, 200, 40), sessionName);
                if (GUI.Button(new Rect(0,40,200,40), "Host")) StartGame(GameMode.Host);
                if (GUI.Button(new Rect(0,80,200,40), "Join")) StartGame(GameMode.Client);
                if (GUI.Button(new Rect(0,120,200,40), "Ninja")) Process.Start("https://www.twitch.tv/ninja");
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                var networkPlayerObject = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
                connectedPlayers[player] = networkPlayerObject;
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (connectedPlayers.TryGetValue(player, out var netObject))
            {
                runner.Despawn(netObject);
                connectedPlayers.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            var kb = Keyboard.current;
            var m = Mouse.current;

            data.movement.x = kb.dKey.ReadValue() - kb.aKey.ReadValue();
            data.movement.y = kb.wKey.ReadValue() - kb.sKey.ReadValue();

            data.run = kb.leftShiftKey.isPressed;
            data.jump = kb.spaceKey.isPressed;

            data.shoot = m.leftButton.isPressed;
            data.aim = m.rightButton.isPressed;

            input.Set(data);
        }
        
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
    }
}