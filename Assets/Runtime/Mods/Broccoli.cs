using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Broccoli : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.maxHealth += 80;
        }
    }
}