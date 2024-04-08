using System;
using System.Text.RegularExpressions;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Runtime.Util
{
    public static class Extension
    {
        public static T Find<T>(this Transform transform, string path) => transform.Find(path, t => t.GetComponent<T>());
        public static T Find<T>(this Transform transform, string path, Func<Transform, T> conversionCallback)
        {
            var find = transform.Find(path);
            return find ? conversionCallback(find) : default;
        }

        public static void ResetPose(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        public static Transform InstanceChild(this Transform transform)
        {
            var child = new GameObject().transform;
            child.SetParent(transform);
            child.ResetPose();
            child.SetAsLastSibling();
            child.name = $"New Child.{child.GetSiblingIndex()}";
            return child;
        }
        
        public static GameObject Find(this GameObject gameObject, string path)
        {
            var find = gameObject.transform.Find(path);
            return find ? find.gameObject : null;
        }

        public static Transform Search(this Transform transform, string pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            var regex = new Regex(pattern, regexOptions);
            foreach (var child in transform.GetComponentsInChildren<Transform>())
            {
                if (regex.IsMatch(child.name)) return child;
            }
            return null;
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }
    }
}