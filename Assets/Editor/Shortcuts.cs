using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Shortcuts : MonoBehaviour
{
    [MenuItem("Goto/Player Avatar")]
    public static void OpenPlayerAvatar() => PrefabStageUtility.OpenPrefab("Assets/Prefabs/Player/Player Avatar.prefab");

    [MenuItem("Goto/Player Instance")]
    public static void OpenPlayerInstance() => PrefabStageUtility.OpenPrefab("Assets/Prefabs/Player/Player Instance.prefab");

    [MenuItem("Goto/Rifle")]
    public static void OpenGun() => PrefabStageUtility.OpenPrefab("Assets/Prefabs/Weapons/Rifle.prefab");
}
