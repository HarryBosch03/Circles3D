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

        public int damage => Mathf.Max(1, Mathf.FloorToInt(baseDamage * damageScale));
        public float GetKnockback(float speed) => damage * baseKnockback * speed * 0.01f;

        public DamageArgs(int baseDamage, float baseKnockback)
        {
            this.baseDamage = baseDamage;
            this.baseKnockback = baseKnockback;
            damageScale = 1f;
        }
    }
}