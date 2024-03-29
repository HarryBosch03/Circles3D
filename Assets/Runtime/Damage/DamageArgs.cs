using System;

namespace Runtime.Damage
{
    [Serializable]
    public struct DamageArgs
    {
        public int damage;
        public float baseKnockback;

        public float GetKnockback(float speed) => damage * baseKnockback * speed * 0.01f;
    }
}