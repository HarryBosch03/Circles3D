using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CleanupMissingScripts : EditorWindow
    {
        [MenuItem("Actions/Cleanup Missing Scripts")]
        public static void Open() { CreateWindow<CleanupMissingScripts>("Cleanup Missing Scripts"); }

        private List<GameObject> prefabs = new();
        private List<GameObject> scene = new();

        private void OnEnable()
        {
            Search();
        }

        private void Search()
        {
            prefabs.Clear();

            var search = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in search)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;

                foreach (var child in prefab.GetComponentsInChildren<Transform>())
                {
                    if (!HasMissingScript(child.gameObject)) continue;

                    prefabs.Add(prefab);
                    break;
                }
            }

            scene.Clear();
            foreach (var transform in FindObjectsOfType<Transform>())
            {
                if (!HasMissingScript(transform.gameObject)) continue;
                scene.Add(transform.gameObject);
            }
        }

        public bool HasMissingScript(GameObject gameObject) { return gameObject.GetComponents<Component>().Any(c => !c); }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"Prefabs [{prefabs.Count}]");
            foreach (var prefab in prefabs)
            {
                EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
            }
            
            EditorGUILayout.LabelField($"Scene [{prefabs.Count}]");
            foreach (var gameObject in scene)
            {
                EditorGUILayout.ObjectField(gameObject, typeof(GameObject), false);
            }
            
            if (GUILayout.Button("Search Again")) Search();
        }
    }
}