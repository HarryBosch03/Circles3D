using Runtime.Stats;

namespace Runtime.Mods
{
    public class Spray : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.attackSpeed *= 4f;
            stats.magazineSize += 13;
            stats.reloadTime += 0.5f;
            stats.spray += 2.0f;
            stats.damage *= 0.4f;
            stats.recoil *= 0.5f;
        }
    }
}