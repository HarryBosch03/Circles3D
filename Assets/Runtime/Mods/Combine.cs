using Runtime.Stats;

namespace Runtime.Mods
{
    public class Combine : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= 3f;
            stats.magazineSize -= 4;
        }
    }
}