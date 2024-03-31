using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runtime.Mods.GunMods;
using UnityEngine;

namespace Runtime.Stats
{
    public class StatBoard : MonoBehaviour
    {
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
        
        private List<Stat> stats = new();
        
        public List<Mod> mods = new();
        
        protected virtual void Awake()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(e => e.FieldType == typeof(Stat));
            foreach (var field in fields)
            {
                stats.Add((Stat)field.GetValue(this));
            }
        }

        protected virtual void FixedUpdate()
        {
            foreach (var stat in stats) stat.Reset();

            foreach (var mod in mods) mod.Apply(this);
        }
    }
}