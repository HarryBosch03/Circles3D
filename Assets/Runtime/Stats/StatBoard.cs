using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runtime.Mods.GunMods;
using UnityEngine;

namespace Runtime.Stats
{
    public class StatBoard : MonoBehaviour
    {
        [Header("Player Stats")]
        public Stat maxHealth = 100f;
        public Stat maxBuffer = 0f;
        
        [Space]
        [Header("Weapon Stats")]
        public Stat damage = 5f;
        public Stat knockback = 0f;
        public Stat projectileSpeed = 120f;
        public Stat spray = 0.5f;
        public Stat attackSpeed = 60f / 300f;
        public Stat magazineSize = 3f;
        public Stat reloadTime = 1.5f;
        public Stat recoil = 1f;
        public Stat bounces = 0f;
        public Stat homing = 0f;
        public Stat projectileLifetime = 5f;
        
        private List<Stat> stats = new();
        public List<Mod> mods = new();

        protected virtual void Awake()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(e => e.FieldType == typeof(Stat));
            foreach (var field in fields)
            {
                stats.Add((Stat)field.GetValue(this));
            }

            float max0(float v) => Mathf.Max(0, v);
        }

        protected virtual void FixedUpdate()
        {
            foreach (var stat in stats) stat.Reset();
            foreach (var mod in mods) mod.Apply(this);
            
            Max(maxHealth, 0);
            Max(maxBuffer, 0);
            Max(projectileSpeed, 0);
            Max(spray, 0);
            Max(attackSpeed, 0);
            Max(magazineSize, 1);
            Max(reloadTime, 0);
            Max(recoil, 0);
            Max(bounces, 0);
            Max(homing, 0);
            Max(projectileLifetime, 0);
            
            if (maxBuffer.AsInt() == 0 && maxHealth.AsInt() == 0)
            {
                maxHealth.value = 1;
            }
        }

        public void Max(Stat stat, float max)
        {
            stat.value = Mathf.Max(stat.value, max);
        }
    }
}