using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Combine : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= 2f;
            stats.magazineSize -= 3;
            stats.reloadTime += 0.5f;
            stats.attackSpeed *= 0.8f;
        }
    }
}