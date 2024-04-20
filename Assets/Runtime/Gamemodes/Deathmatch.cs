using Circles3D.Runtime.Player;
using Fusion;

namespace Circles3D.Runtime.Gamemodes
{
    public class Deathmatch : Gamemode
    {
        [Networked]
        public NetworkDictionary<PlayerRef, PlayerMetadata> scoreboard => default;
        
        public override bool CanRespawn(PlayerInstance player) => false;

        public override void FixedUpdateNetwork()
        {
            var playersAlive = 0;
            foreach (var pair in players)
            {
                if (pair.Value.avatar.health.alive) playersAlive++;
            }

            if (playersAlive == 1)
            {
                
            }
        }

        public struct PlayerMetadata : INetworkStruct
        {
            public int rounds;
            public int sets;
        }
    }
}