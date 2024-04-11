using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Mods
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Mod List")]
    public class ModList : ScriptableObject
    {
        public List<Mod> mods = new();

        public Mod Find(string identifier)
        {
            foreach (var mod in mods)
            {
                if (mod.IdentifiesAs(identifier)) return mod;
            }
            return null;
        }
    }
}