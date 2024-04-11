using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Stoneskin : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.maxBuffer += 4;
            stats.maxHealth -= 100;
        }
    }
}