using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Circles3D.Runtime.Networking
{
    public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        public float mouseSensitivity = 0.3f;

        private NetInput input;
        private bool resetInput;
        private Camera mainCam;

        public static bool isControllingPlayer { get; private set; } = true;

        public static void SetIsControllingPlayer(bool state)
        {
            isControllingPlayer = state;
            Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        private void Awake() { mainCam = Camera.main; }

        public static Vector2 SwizzleMouseDelta(Vector2 mouseDelta) => new Vector2(-mouseDelta.y, mouseDelta.x);
        
        public void BeforeUpdate()
        {
            if (resetInput)
            {
                resetInput = false;
                input = default;
            }

            var kb = Keyboard.current;
            var m = Mouse.current;

            var buttons = new NetworkButtons();

            if (isControllingPlayer)
            {
                input.movement.x = kb.dKey.ReadValue() - kb.aKey.ReadValue();
                input.movement.y = kb.wKey.ReadValue() - kb.sKey.ReadValue();
                input.movement = Vector2.ClampMagnitude(input.movement, 1f);

                var mouseDelta = m.delta.ReadValue();
                input.mouseDelta += SwizzleMouseDelta(mouseDelta);

                buttons.Set(NetInput.Jump, kb.spaceKey.isPressed);
                buttons.Set(NetInput.Run, kb.leftShiftKey.isPressed);
                buttons.Set(NetInput.Shoot, m.leftButton.isPressed);
                buttons.Set(NetInput.Aim, m.rightButton.isPressed);
                buttons.Set(NetInput.Block, m.middleButton.isPressed);
            }

            input.buttons = new NetworkButtons(input.buttons.Bits | buttons.Bits);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(this.input);
            resetInput = true;

            this.input.mouseDelta = default;
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { SceneManager.LoadScene(0); }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}