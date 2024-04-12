using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Player;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Gamemodes
{
    public abstract class Gamemode : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        public Transform spawnpointParent;
        public NetworkPrefabRef playerPrefab;
        public string iconName;

        [Space]
        public bool allowPlayerHealthRegeneration;

        [Networked, Capacity(8)]
        public NetworkDictionary<PlayerRef, PlayerInstance> players => default;

        public static Gamemode current { get; private set; }

        protected virtual void OnEnable()
        {
            current = this;
            PlayerAvatar.allowPlayerHealthRegeneration = allowPlayerHealthRegeneration;
        }

        protected virtual void OnDisable()
        {
            if (current == this) current = null;
            PlayerAvatar.allowPlayerHealthRegeneration = null;
        }

        public abstract bool CanRespawn(PlayerInstance player);

        public void RespawnPlayer(PlayerInstance player)
        {
            if (!HasStateAuthority) return;
            if (!CanRespawn(player)) return;

            var spawnpoint = GetSpawnpoint(player);
            player.SpawnAt(spawnpoint.position, spawnpoint.rotation);
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                foreach (var player in players)
                {
                    if (player.Value.avatar.transform.position.y < -50f)
                    {
                        player.Value.avatar.health.Kill(null, new DamageArgs(), Vector3.zero, Vector3.zero);
                    }
                }
            }
        }

        private Transform GetSpawnpoint(PlayerInstance player)
        {
            var scoredPoints = new List<(Transform sp, float score)>();
            foreach (Transform sp in EnumerateSpawnpoints())
            {
                var score = 0f;
                foreach (var other in players)
                {
                    var instance = other.Value;
                    if (instance == player) continue;

                    score = Mathf.Max((instance.avatar.transform.position - sp.transform.position).sqrMagnitude, score);
                }

                scoredPoints.Add((sp, score));
            }

            return scoredPoints.OrderBy(e => e.score).ElementAt(Random.Range(0, Mathf.Min(3, scoredPoints.Count))).sp;
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

        protected PlayerRef? GetPlayerFromAvatar(PlayerAvatar avatar)
        {
            if (!avatar) return null;
            foreach (var e in players)
            {
                if (e.Value == avatar.owningPlayerInstance) return e.Key;
            }
            return null;
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