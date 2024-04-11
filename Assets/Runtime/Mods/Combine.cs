using Runtime.Stats;

namespace Runtime.Mods
{
    public class Combine : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.bulletCount *= 2;
            stats.spray += 1f;
            stats.magazineSize -= 3;
        }
    }
}