using Runtime.Stats;

namespace Runtime.Mods.GunMods
{
    public class Homing : Mod
    {
        public override void Apply(StatBoard statBoard)
        {
            statBoard.homing.value += 1f;
            statBoard.projectileSpeed.value *= 0.25f;
            statBoard.damage.value *= 0.5f;
            statBoard.reloadTime.value += 0.25f;
        }
    }
}