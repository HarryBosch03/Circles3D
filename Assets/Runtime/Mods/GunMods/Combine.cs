using Runtime.Stats;

namespace Runtime.Mods.GunMods
{
    public class Combine : Mod<GunStatBoard>
    {
        public override void OnApply(GunStatBoard gun)
        {
            gun.damage.value *= gun.magazineSize.value;
            gun.magazineSize.value = 1;
        }
    }
}