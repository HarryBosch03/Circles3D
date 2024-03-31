using GameKit.Utilities;
using UnityEngine;

namespace Runtime.Util
{
    public static class Extension
    {
        public static T Find<T>(this Transform transform, string path)
        {
            var find = transform.Find(path);
            return find ? find.GetComponent<T>() : default;
        }
        
        public static GameObject Find(this GameObject gameObject, string path)
        {
            var find = gameObject.transform.Find(path);
            return find ? find.gameObject : null;
        }
        
        public static Transform Search(this Transform transform, string name)
        {
            foreach (var child in transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == name) return child;
            }
            return null;
        }
    }
}