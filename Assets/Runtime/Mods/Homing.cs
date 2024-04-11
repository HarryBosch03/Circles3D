using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Homing : Mod
    {
        public float strength = 6f;

        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.homing += strength;
            stats.bulletSpeed *= 0.6f;
            stats.damage *= 0.5f;
            stats.reloadTime += 0.25f;
        }
    }
}