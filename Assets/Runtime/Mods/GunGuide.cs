using Circles3D.Runtime.Stats;

namespace Circles3D.Runtime.Mods
{
    public class GunGuide : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.reloadTime *= 0.5f;
        }
    }
}