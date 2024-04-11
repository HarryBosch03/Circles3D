using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class Armored : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.maxBuffer++;
            stats.moveSpeed *= 0.9f;
        }
    }
}