using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Circles3D.Editor
{
    public static class Actions
    {
        [MenuItem("Actions/Open Build")]
        public static void OpenBuild()
        {
            var path = Path.Combine("./", "Builds", "Circles3D.exe");
            if (File.Exists(path))
            {
                Debug.Log("Starting Build...");
                Process.Start(path);
            }
            else Debug.LogError($"No build found at \"{path}\"");
        }
    }
}