using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class Shortcuts : MonoBehaviour
{
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player Avatar.prefab";

    private static List<GameObject> tempPrefabInstances = new();

    static Shortcuts()
    {
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;
    }

    [MenuItem(Goto.PlayerPrefab.InIsolation)]
    public static void OpenPlayerInIsolation()
    {
        PrefabStageUtility.OpenPrefab(PlayerPrefabPath);
    }
    
    [MenuItem(Goto.PlayerPrefab.InContext)]
    public static void OpenPlayerInContext()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = $"[TEMP]{prefab.name}";

        tempPrefabInstances.Add(instance);
        
        PrefabStageUtility.OpenPrefab(PlayerPrefabPath, instance, PrefabStage.Mode.InContext);
    }
    
    [MenuItem(Goto.PlayerPrefab.Select)]
    public static void SelectPlayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        EditorGUIUtility.PingObject(prefab);
        
        Selection.objects = new Object[] { prefab };
        Selection.activeObject = prefab;
    }

    private static void OnPrefabStageClosing(PrefabStage stage)
    {
        foreach (var tempInstance in tempPrefabInstances)
        {
            DestroyImmediate(tempInstance);
        }
        tempPrefabInstances.Clear();
    }

    public static class Goto
    {
        public const string path = "Goto/";

        public static class PlayerPrefab
        {
            public const string path = "Player Avatar/";

            public const string InIsolation = Goto.path + path + "In Isolation";
            public const string InContext = Goto.path + path + "In Context";
            public const string Select = Goto.path + path + "Select";
        }
    }
}
