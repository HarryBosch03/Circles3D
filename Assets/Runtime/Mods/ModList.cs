using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Mods
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Mod List")]
    public class ModList : ScriptableObject
    {
        public List<Mod> mods = new();

        public Mod Find(string s)
        {
            foreach (var mod in mods)
            {
                if (mod.name.ToLower().Trim() == s.ToLower().Trim()) return mod;
            }
            return null;
        }
    }
}