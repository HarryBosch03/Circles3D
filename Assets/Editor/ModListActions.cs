using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Circles3D.Runtime.Mods;
using Fusion;
using UnityEditor;
using UnityEngine;

namespace Circles3D.Editor
{
    public static class ModListActions
    {
        [MenuItem("Actions/Mods/Rebuild Mod List")]
        public static void RebuildModList()
        {
            var mods = GetAllModPrefabs();
            var modList = FindModList();

            Undo.RecordObject(modList, "Rebuilt Mod List");
            Debug.Log($"Rebuilt mods list [From: {modList.mods.Count}, To: {mods.Count}]");
            modList.mods = mods;
        }

        private static ModList FindModList()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var modList = AssetDatabase.LoadAssetAtPath<ModList>(AssetDatabase.GUIDToAssetPath(guid));
                if (modList) return modList;
            }
            return null;
        }

        private static List<Mod> GetAllModPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var mods = new List<Mod>();
            foreach (var guid in guids)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (!prefab) continue;

                var mod = prefab.GetComponent<Mod>();
                if (!mod) continue;

                mods.Add(mod);
            }
            return mods;
        }

        [MenuItem("Actions/Mods/Create Missing Mods")]
        public static void CreateMissingMods()
        {
            var modTypes = typeof(Mod).Assembly.GetTypes().Where(e => e.IsSubclassOf(typeof(Mod)) && !e.IsAbstract).ToArray();
            foreach (var type in modTypes)
            {
                var path = $"Assets/Prefabs/Mods/{type.Name}.prefab";
                if (File.Exists($"./{path}")) continue;
                
                Debug.Log($"Creating Prefab for Mod \"{type.Name}\"");
                var newModInstance = new GameObject(type.Name);
                try
                {
                    newModInstance.AddComponent(type);
                    newModInstance.AddComponent<NetworkObject>();
                    PrefabUtility.SaveAsPrefabAsset(newModInstance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    Object.DestroyImmediate(newModInstance);
                }
            }
        }

        [MenuItem("Actions/Mods/Write Mod List")]
        private static void WriteModList()
        {
            var mods = GetAllModPrefabs();
            var file = new StringBuilder();

            file.AppendLine("# Current Mod List");
            file.AppendLine("|Index|Identifier|Display Name|Description|Changes|");
            file.AppendLine("|----:|:---------|:-----------|:----------|:------|");
            for (var i = 0; i < mods.Count; i++)
            {
                var mod = mods[i];
                file.AppendLine($"|{i + 1}|{mod.name}|{mod.displayName}|{mod.description}|{mod.GetChangeList()}");
            }

            var path = "./mod-list.md";
            File.WriteAllText(path, file.ToString());
            Debug.Log($"Written Mods List to \"{Path.GetFullPath(path)}\"");
        }
    }
}