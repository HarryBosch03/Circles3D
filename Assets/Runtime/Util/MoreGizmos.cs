using UnityEngine;

namespace Circles3D.Runtime.Util
{
    public static class MoreGizmos
    {
        public static void DrawAxis(Vector3 position, Quaternion rotation, float length, float centerSize = 0f)
        {
            var baseColor = Gizmos.color;

            drawAxis(Vector3.right, Color.red);
            drawAxis(Vector3.up, Color.green);
            drawAxis(Vector3.forward, Color.blue);


            Gizmos.color = baseColor * Color.yellow;
            Gizmos.DrawSphere(position, length * centerSize);
            
            void drawAxis(Vector3 direction, Color color)
            {
                Gizmos.color = baseColor * color;
                Gizmos.DrawRay(position, rotation * direction * length);
            }
        }
    }
}