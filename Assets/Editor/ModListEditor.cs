using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fusion;
using Runtime.Mods;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ModListEditor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
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
                    var prefab = PrefabUtility.SaveAsPrefabAsset(newModInstance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                finally
                {
                    Object.DestroyImmediate(newModInstance);
                }
            }
                            
            var mods = new List<Mod>();
            foreach (var path in Directory.EnumerateFiles("./Assets/Prefabs/Mods", "*.prefab", SearchOption.AllDirectories))
            {
                var assetPath = path[2..];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var mod = prefab.GetComponent<Mod>();
                mods.Add(mod);
            }
            
            var search = AssetDatabase.FindAssets($"t:{nameof(ModList)}");
            foreach (var guid in search)
            {
                var modList = AssetDatabase.LoadAssetAtPath<ModList>(AssetDatabase.GUIDToAssetPath(guid));
                if (!modList) continue;
                modList.mods.Clear();
                modList.mods.AddRange(mods);
                EditorUtility.SetDirty(modList);
            }
        }
    }
}