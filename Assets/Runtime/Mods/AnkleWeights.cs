using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class AnkleWeights : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.moveSpeed *= 1.5f;
            stats.acceleration *= 1f / 3f;
        }
    }
}