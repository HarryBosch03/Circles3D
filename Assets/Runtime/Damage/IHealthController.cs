using System;

namespace Circles3D.Runtime.Damage
{
    public interface IHealthController : IDamageable
    {
        public int currentHealth { get; }
        public int currentBuffer { get; }
        public int maxHealth { get; }
        public int maxBuffer { get; }
        public event Action HealthChangedEvent;

        public float GetHealthFactor();
    }
}