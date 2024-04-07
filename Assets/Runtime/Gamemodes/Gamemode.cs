using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Runtime.Player;
using UnityEngine;

namespace Runtime.Gamemodes
{
    public abstract class Gamemode : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public Transform spawnpointParent;
        public NetworkPrefabRef playerPrefab;
        public string iconName;

        [Networked, Capacity(8)]
        private NetworkDictionary<PlayerRef, PlayerInstance> players => default;

        public static Gamemode current { get; private set; }

        private void OnEnable() { current = this; }

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
            player.SpawnAt(spawnpoint.position, spawnpoint.rotation);
        }

        private Transform GetSpawnpoint(PlayerInstance player)
        {
            var best = (Transform)null;
            var bestScore = -1f;
            foreach (Transform sp in EnumerateSpawnpoints())
            {
                var score = 0f;
                foreach (var other in players)
                {
                    var instance = other.Value;
                    if (instance == player) continue;

                    score = Mathf.Max(1f / (instance.avatar.transform.position - sp.transform.position).sqrMagnitude, score);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = sp;
                }
            }

            return best ? best : transform;
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

        private void OnDrawGizmos()
        {
            foreach (Transform sp in EnumerateSpawnpoints())
            {
                Gizmos.DrawIcon(sp.position, iconName);
            }
        }

        public IEnumerable EnumerateSpawnpoints() => spawnpointParent ? spawnpointParent : new object[] { transform };
    }
}