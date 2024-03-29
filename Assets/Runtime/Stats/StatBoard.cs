using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runtime.Mods.GunMods;
using UnityEngine;

namespace Runtime.Stats
{
    public abstract class StatBoard : MonoBehaviour
    {
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