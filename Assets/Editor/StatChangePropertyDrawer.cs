using System.Linq;
using Circles3D.Runtime.Mods;
using Circles3D.Runtime.Stats;
using UnityEditor;
using UnityEngine;

namespace Circles3D.Editor
{
    [CustomPropertyDrawer(typeof(Mod.StatChange))]
    public class StatChangePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight + 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            position.y++;
            
            var rect = position;

            var props = new TargetLayout(property);

            var stats = StatBoard.Stats.Metadata.Keys.ToList();
            
            var splitSize = 100;
            EditorGUI.PropertyField(next(position.width - 200), props.value, GUIContent.none);
            EditorGUI.PropertyField(next(100), props.changeType, GUIContent.none);

            var index = stats.IndexOf(props.fieldName.stringValue);
            if (index == -1) index = 0;
            index = EditorGUI.Popup(next(100), index, stats.ToArray());
            props.fieldName.stringValue = stats[index];
            
            Rect next(float width)
            {
                rect.width = width;
                var val = rect;
                rect.x += rect.width;

                val.x++;
                val.width--;
                
                return val;
            }
        }

        private struct TargetLayout
        {
            public SerializedProperty fieldName;
            public SerializedProperty metadata;
            public SerializedProperty changeType;
            public SerializedProperty value;
            
            public TargetLayout(SerializedProperty root)
            {
                fieldName = root.FindPropertyRelative(nameof(fieldName));
                metadata = root.FindPropertyRelative(nameof(metadata));
                changeType = root.FindPropertyRelative(nameof(changeType));
                value = root.FindPropertyRelative(nameof(value));
            }
        }
    }
}