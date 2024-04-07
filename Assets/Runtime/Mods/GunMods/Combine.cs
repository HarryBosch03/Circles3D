using Runtime.Stats;

namespace Runtime.Mods.GunMods
{
    public class Combine : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= stats.magazineSize;
            stats.magazineSize = 1;
        }
    }
}