using Runtime.Stats;

namespace Runtime.Mods
{
    public class Homing : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.homing += 1f;
            stats.projectileSpeed *= 0.25f;
            stats.damage *= 0.5f;
            stats.reloadTime += 0.25f;
        }
    }
}