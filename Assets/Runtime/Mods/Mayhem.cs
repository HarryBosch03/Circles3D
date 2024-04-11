using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Mayhem : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.bounces += 2;
            stats.damage *= 0.8f;
            stats.spray += 2f;
            stats.bulletSpeed *= 0.8f;
        }
    }
}