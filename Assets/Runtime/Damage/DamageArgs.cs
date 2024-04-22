using System;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Damage
{
    [Serializable]
    public struct DamageArgs : INetworkStruct
    {
        public int baseDamage;
        public float damageScale;
        public float baseKnockback;
        public bool ignoreLocationalDamage;

        public float knockback => damageScale * (1f + baseKnockback);
        public int damage => Mathf.Max(1, Mathf.FloorToInt(baseDamage * damageScale));

        public DamageArgs(float baseDamage, float baseKnockback, bool ignoreLocationalDamage = false)
        {
            this.baseDamage = (int)baseDamage;
            this.baseKnockback = baseKnockback;
            this.ignoreLocationalDamage = ignoreLocationalDamage;
            damageScale = 1f;
        }
    }
}