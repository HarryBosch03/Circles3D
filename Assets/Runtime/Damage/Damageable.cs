using UnityEngine;

namespace Runtime.Damage
{
    public interface IDamageable
    {
        void Damage(DamageArgs args, Vector3 point, Vector3 direction);
    }
}