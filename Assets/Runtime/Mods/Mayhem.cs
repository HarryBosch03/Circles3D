using Runtime.Stats;

namespace Runtime.Mods
{
    public class Mayhem : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.bounces += 5;
            stats.damage *= 0.8f;
        }
    }
}