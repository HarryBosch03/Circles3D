using Runtime.Weapons;

namespace Runtime.Stats
{
    public class GunStatBoard : StatBoard
    {
        public Stat damage = 5f;
        public Stat knockback = 0f;
        public Stat projectileSpeed = 120f;
        public Stat spray = 0.5f;
        public Stat attackSpeed = 60f / 300f;
        public Stat magazineSize = 3f;
        public Stat reloadTime = 1.5f;
        public Stat recoil = 1f;
    }
}