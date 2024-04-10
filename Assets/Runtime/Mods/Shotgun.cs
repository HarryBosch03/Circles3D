using Runtime.Stats;

namespace Runtime.Mods
{
    public class Shotgun : Mod
    {
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.damage *= 0.25f;
            stats.bulletCount += 11;
            stats.attackSpeed *= 0.6f;
            stats.magazineSize -= 4;
            stats.spray += 2.0f;
        }
    }
}