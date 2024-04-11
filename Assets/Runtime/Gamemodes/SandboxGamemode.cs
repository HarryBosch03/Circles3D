using Circles3D.Runtime.Player;

namespace Circles3D.Runtime.Gamemodes
{
    public class SandboxGamemode : Gamemode
    {
        public override bool CanRespawn(PlayerInstance player) => true;
    }
}
