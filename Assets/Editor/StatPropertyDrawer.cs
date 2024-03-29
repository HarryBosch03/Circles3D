using Runtime.Stats;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(Stat), true)]
    public class StatPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            
            rect.width -= 50;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(Stat.baseValue)), label);
            
            EditorGUI.BeginDisabledGroup(true);
            rect.x += rect.width;
            rect.width = 50;
            EditorGUI.FloatField(rect, property.FindPropertyRelative(nameof(Stat.value)).floatValue);
            EditorGUI.EndDisabledGroup();
        }
    }
}
