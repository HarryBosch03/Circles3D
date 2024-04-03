using Runtime.Player;

namespace Runtime.Gamemodes
{
    public class SandboxGamemode : Gamemode
    {
        public PlayerInstance playerPrefab;

        public override bool CanRespawn(PlayerInstance player) => true;
    }
}
