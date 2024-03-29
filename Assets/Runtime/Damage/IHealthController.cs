namespace Runtime.Damage
{
    public interface IHealthController : IDamageable
    {
        public int GetCurrentHealth();
        public int GetMaxHealth();
        public int GetCurrentBuffer();
        public int GetMaxBuffer();
    }
}