using System.Collections.Generic;
using Fusion;
using Runtime.Mods;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Stats
{
    public class StatBoard : NetworkBehaviour
    {
        public ModList modList;
        public Stats baseStats = Stats.Defaults;
        
        [Networked] private Stats evaluatedInternal { get; set; }
        public Stats evaluated => Runner ? evaluatedInternal : baseStats;
     
        public List<Mod> mods = new();

        public override void Spawned()
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
            Max(ref stats.bulletSpeed, 0);
            Max(ref stats.bulletCount, 1);
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

            evaluatedInternal = stats;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void AddModRpc(string modName)
        {
            var mod = modList.Find(modName);
            var instance = Runner.Spawn(mod);
            instance.name = mod.name;
            instance.SetOwnerRpc(this);
        }
        
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RemoveModRpc(string modName)
        {
            var mod = mods.Find(mod => mod.name.ToLower().Trim() == modName.ToLower().Trim());
            Runner.Despawn(mod.Object);
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
                bulletSpeed = 200f,
                bulletCount = 1,
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
            [FormerlySerializedAs("projectileSpeed")] 
            public float bulletSpeed;
            public int bulletCount;
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