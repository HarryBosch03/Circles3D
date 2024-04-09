using Runtime.Player;

namespace Runtime.Gamemodes
{
    public class SandboxGamemode : Gamemode
    {
        public override bool CanRespawn(PlayerInstance player) => true;
    }
}
