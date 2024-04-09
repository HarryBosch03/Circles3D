using Runtime.Gamemodes;
using Runtime.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(Gamemode), editorForChildClasses: true)]
public class GamemodeEditor : UnityEditor.Editor
{
    private Gamemode gamemode => target as Gamemode;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Validate Spawnpoints"))
        {
            MoveSpawnpointsToGround();
        }
    }

    private void OnSceneGUI()
    {
        foreach (Transform sp in gamemode.EnumerateSpawnpoints())
        {
            EditorGUI.BeginChangeCheck();
            var position = Handles.PositionHandle(sp.position, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sp, $"Moved {sp.name}");
                sp.position = position;
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    MoveSpawnpointToGround(sp);
                }
            }

            var ray = new Ray(sp.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out var hit))
            {
                var radius = Mathf.Sqrt(Mathf.Abs(hit.distance - 1.0f));
                Handles.color = hit.distance > 1f ? Color.green : Color.magenta;
                Handles.DrawLine(sp.position, hit.point);
                Handles.DrawLine(hit.point, hit.point + Vector3.forward * radius);
                Handles.DrawWireArc(hit.point, Vector3.up, Vector3.forward, 360f, radius);
            }
        }
    }
    
    public void MoveSpawnpointsToGround()
    {
        var spawnpoints = gamemode.EnumerateSpawnpoints();
        foreach (Transform sp in spawnpoints)
        {
            MoveSpawnpointToGround(sp);
        }

        gamemode.spawnpointParent.SetParent(null);
        gamemode.transform.ResetPose();
        gamemode.spawnpointParent.SetParent(gamemode.transform);
    }

    public void MoveSpawnpointToGround(Transform sp)
    {
        var ray = new Ray(sp.position + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out var hit))
        {
            sp.position = hit.point;
        }
    }
}