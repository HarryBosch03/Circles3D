using Runtime.Stats;

namespace Runtime.Mods.GunMods
{
    public class Combine : Mod
    {
        public override void Apply(StatBoard gun)
        {
            gun.damage.value *= gun.magazineSize.value;
            gun.magazineSize.value = 1;
        }
    }
}