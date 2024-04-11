using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Combine : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= 2f;
            stats.spray += 1f;
            stats.magazineSize -= 3;
            stats.bulletSpeed *= 1.5f;
        }
    }
}