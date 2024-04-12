using System.Collections.Generic;
using Circles3D.Runtime.Mods;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Circles3D.Runtime.Stats
{
    public class StatBoard : NetworkBehaviour
    {
        public ModList modList;
        public Stats baseStats = Stats.Defaults;

        [Networked] private Stats evaluatedInternal { get; set; }
        public Stats evaluated => Runner ? evaluatedInternal : baseStats;

        public List<Mod> mods = new();

        public override void Spawned() { UpdateStats(); }

        public override void FixedUpdateNetwork() { UpdateStats(); }

        private void UpdateStats()
        {
            var stats = baseStats;
            foreach (var mod in mods) mod.Apply(ref stats);

            Max(ref stats.maxHealth, 0);
            Max(ref stats.maxBuffer, 0);
            Max(ref stats.bulletSpeed, 20);
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
        public void AddModRpc(string identifier)
        {
            var mod = modList.Find(identifier);
            var instance = Runner.Spawn(mod);
            instance.transform.SetParent(transform);
            RegisterModRpc(instance.Object);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
        private void RegisterModRpc(NetworkId modInstanceId)
        {
            var mod = Runner.FindObject(modInstanceId).GetComponent<Mod>();
            mod.SetParent(this);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RemoveModRpc(string modName)
        {
            var mod = mods.Find(mod => mod.IdentifiesAs(modName));
            if (mod) Runner.Despawn(mod.Object);
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
                moveSpeed = 8f,
                acceleration = 10f,
                gravity = 1f,

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

            public static readonly Dictionary<string, StatMetadata> Metadata = new Dictionary<string, StatMetadata>()
            {
                { nameof(maxHealth), new StatMetadata("Max Health") },
                { nameof(maxBuffer), new StatMetadata("Max Buffer") },
                { nameof(moveSpeed), new StatMetadata("Move Speed") },
                { nameof(acceleration), new StatMetadata("Move Acceleration") },
                { nameof(gravity), new StatMetadata("Player Gravity", StatMetadata.Connotation.Neutral) },
                { nameof(damage), new StatMetadata("Damage") },
                { nameof(knockback), new StatMetadata("Knockback") },
                { nameof(bulletSpeed), new StatMetadata("Bullet Speed") },
                { nameof(bulletCount), new StatMetadata("Bullet Count") },
                { nameof(spray), null },
                { nameof(attackSpeed), new StatMetadata("Attack Speed") },
                { nameof(magazineSize), new StatMetadata("Magazine Size") },
                { nameof(reloadTime), new StatMetadata("Reload Time", StatMetadata.Connotation.Reversed) },
                { nameof(recoil), null },
                { nameof(bounces), new StatMetadata("Bounces") },
                { nameof(homing), null },
                { nameof(projectileLifetime), null },
            };

            public static StatMetadata GetMetadata(string fieldName) => Metadata.GetValueOrDefault(fieldName);

            [Header("Player Stats")]
            public float maxHealth;
            public float maxBuffer;
            public float moveSpeed;
            public float acceleration;
            public float gravity;

            [Header("Projectile Stats")]
            public float damage;
            public float knockback;
            public float bulletSpeed;
            public float bulletCount;
            public float spray;
            public float attackSpeed;
            public float magazineSize;
            public float reloadTime;
            public float recoil;
            public float bounces;
            public float homing;
            public float projectileLifetime;

            public class StatMetadata
            {
                public string displayName;
                public Connotation connotation;

                public StatMetadata(string displayName, Connotation connotation = Connotation.Normal)
                {
                    this.displayName = displayName;
                    this.connotation = connotation;
                }
                
                public enum Connotation
                {
                    Normal,
                    Reversed,
                    Neutral,
                }
            }
        }
    }
}