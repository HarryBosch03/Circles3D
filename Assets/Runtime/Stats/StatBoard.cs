using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fusion;
using Runtime.Mods.GunMods;
using UnityEngine;

namespace Runtime.Stats
{
    public class StatBoard : NetworkBehaviour
    {
        public Stats baseStats;
        
        [Networked]
        public Stats evaluated { get; private set; }
     
        public List<Mod> mods = new();

        protected virtual void Awake()
        {
            UpdateStats();
        }

        public override void FixedUpdateNetwork() { UpdateStats(); }

        private void UpdateStats()
        {
            var stats = baseStats;
            foreach (var mod in mods) mod.Apply(ref stats);

            Max(ref stats.maxHealth, 0);
            Max(ref stats.maxBuffer, 0);
            Max(ref stats.projectileSpeed, 0);
            Max(ref stats.spray, 0);
            Max(ref stats.attackSpeed, 0);
            Max(ref stats.magazineSize, 1);
            Max(ref stats.reloadTime, 0);
            Max(ref stats.recoil, 0);
            Max(ref stats.bounces, 0);
            Max(ref stats.homing, 0);
            Max(ref stats.projectileLifetime, 0);

            if (stats.maxBuffer == 0 && stats.maxHealth == 0)
            {
                stats.maxHealth = 1;
            }

            this.evaluated = stats;
        }

        public void Max(ref int stat, int max) { stat = Mathf.Max(stat, max); }
        public void Max(ref float stat, float max) { stat = Mathf.Max(stat, max); }

        [System.Serializable]
        public struct Stats : INetworkStruct
        {
            public static readonly Stats Defaults = new Stats
            {
                maxHealth = 100,
                maxBuffer = 0,
                damage = 45,
                knockback = 0f,
                projectileSpeed = 200f,
                spray = 0.5f,
                attackSpeed = 5f,
                magazineSize = 7,
                reloadTime = 1.5f,
                recoil = 1f,
                bounces = 0,
                homing = 0f,
                projectileLifetime = 5f,
            };

            public int maxHealth;
            public int maxBuffer;
            public float damage;
            public float knockback;
            public float projectileSpeed;
            public float spray;
            public float attackSpeed;
            public int magazineSize;
            public float reloadTime;
            public float recoil;
            public int bounces;
            public float homing;
            public float projectileLifetime;
        }
    }
}