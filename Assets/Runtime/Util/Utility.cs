using UnityEngine;

namespace Circles3D.Runtime.Util
{
    public static class Utility
    {
        public static void EnsureChildCount(Transform parent, int count)
        {
            while (parent.childCount < count) parent.InstanceChild();
        }
    }
}