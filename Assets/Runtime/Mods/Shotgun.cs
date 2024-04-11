using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Shotgun : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= 0.35f;
            stats.bulletCount += 8;
            stats.attackSpeed *= 0.6f;
            stats.magazineSize -= 4;
            stats.spray += 4.0f;
            stats.reloadTime += 0.5f;
        }
    }
}