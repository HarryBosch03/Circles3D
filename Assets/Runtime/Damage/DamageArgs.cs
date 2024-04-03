using System;
using UnityEngine;

namespace Runtime.Damage
{
    [Serializable]
    public struct DamageArgs
    {
        public int baseDamage;
        public float damageScale;
        public float baseKnockback;

        public float knockback => damageScale * (1f + baseKnockback);
        public int damage => Mathf.Max(1, Mathf.FloorToInt(baseDamage * damageScale));

        public DamageArgs(int baseDamage, float baseKnockback)
        {
            this.baseDamage = baseDamage;
            this.baseKnockback = baseKnockback;
            damageScale = 1f;
        }
    }
}