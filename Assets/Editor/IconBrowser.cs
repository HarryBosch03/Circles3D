using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class IconBrowser : EditorWindow
    {
        [MenuItem("Tools/Icon Browser")]
        public static void Open() => CreateWindow<IconBrowser>("Icon Browser");

        private Vector2 scrollPosition;
        private IEnumerable<string> allIcons;
        private string selection;

        private void OnEnable() { allIcons = IconMiner.EnumerateIcons(); }

        private void OnGUI()
        {
            var iconSize = 80.0f;

            using (var sv = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandHeight(true)))
            {
                scrollPosition = sv.scrollPosition;
                var width = position.width - 18;
                var rows = Mathf.FloorToInt(width / iconSize);
                iconSize = width / rows;
                var columns = Mathf.CeilToInt(allIcons.Count() / (float)rows);

                var rect = EditorGUILayout.GetControlRect(false, columns * iconSize);
                var i = 0;
                foreach (var name in allIcons)
                {
                    var content = EditorGUIUtility.IconContent(name);
                    content.text = null;

                    var row = i % rows;
                    var column = i / rows;
                    var subrect = new Rect(rect.x + iconSize * row, rect.y + iconSize * column, iconSize, iconSize);
                    if (GUI.Button(subrect, content)) selection = name;
                    i++;
                }
            }

            if (selection != null)
            {
                var content = EditorGUIUtility.IconContent(selection);
                content.text = $"Selected Icon: \"{selection}\"";
                if (GUILayout.Button(content, GUILayout.Height(80))) Debug.Log(selection);
            }
        }
    }
}