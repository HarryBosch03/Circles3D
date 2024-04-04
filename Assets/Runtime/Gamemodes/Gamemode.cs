using Runtime.Player;
using UnityEngine;

namespace Runtime.Gamemodes
{
    public abstract class Gamemode : MonoBehaviour
    {
        public Transform spawnpointParent;
        
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
            if (!CanRespawn(player)) return;

            var spawnpoint = GetSpawnpoint(player);
            player.SpawnAt(spawnpoint.position, spawnpoint.rotation);
        }

        private Transform GetSpawnpoint(PlayerInstance player)
        {
            if (!spawnpointParent) return transform;
            if (spawnpointParent.childCount == 0) return spawnpointParent;
            
            var child = spawnpointParent.GetChild(Random.Range(0, spawnpointParent.childCount));
            return child;
        }
    }
}