using System.Collections.Generic;
using UnityEngine;

namespace Circles3D.Runtime.Mods
{
    public static class ModList
    {
        private static List<Mod> mods_Internal;
        
        public static List<Mod> mods
        {
            get
            {
                if (mods_Internal != null) return mods_Internal;
                mods_Internal = new List<Mod>(Resources.LoadAll<Mod>("Mods"));
                return mods_Internal;
            }
        }

        public static Mod Find(string name)
        {
            foreach (var mod in mods)
            {
                if (mod.IdentifiesAs(name)) return mod;
            }
            return null;
        }
    }
}