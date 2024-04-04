using Fusion;
using Runtime.Player;
using UnityEngine;

namespace Runtime.Gamemodes
{
    public abstract class Gamemode : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public Transform spawnpointParent;
        public NetworkPrefabRef playerPrefab;

        [Networked, Capacity(8)]
        private NetworkDictionary<PlayerRef, PlayerInstance> players => default;
        
        public static Gamemode current { get; private set; }
        
        private void OnEnable()
        {
            current = this;
        }

        private void OnDisable()
        {
            if (current == this) current = null;
        }

        public abstract bool CanRespawn(PlayerInstance player);
        
        public void RespawnPlayer(PlayerInstance player)
        {
            if (!HasStateAuthority) return;
            if (!CanRespawn(player)) return;

            var spawnpoint = GetSpawnpoint(player);
            RpcSpawnPlayerAt(player.Object, spawnpoint.position, spawnpoint.rotation);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcSpawnPlayerAt(NetworkObject playerObject, Vector3 position, Quaternion rotation)
        {
            var player = playerObject.GetComponent<PlayerInstance>();
            player.SpawnAt(position, rotation);
        }

        private Transform GetSpawnpoint(PlayerInstance player)
        {
            if (!spawnpointParent) return transform;
            if (spawnpointParent.childCount == 0) return spawnpointParent;
            
            var child = spawnpointParent.GetChild(Random.Range(0, spawnpointParent.childCount));
            return child;
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority) return;

            var netPlayer = Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
            players.Add(player, netPlayer.GetComponent<PlayerInstance>());
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;

            if (players.TryGet(player, out var playerBehaviour))
            {
                players.Remove(player);
                Runner.Despawn(playerBehaviour.Object);
            }
        }
    }
}