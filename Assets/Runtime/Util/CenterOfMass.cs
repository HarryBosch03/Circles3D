using UnityEngine;

namespace Circles3D.Runtime.Util
{
    public class CenterOfMass : MonoBehaviour
    {
        private void OnEnable()
        {
            var body = GetComponentInParent<Rigidbody>();
            if (!body)
            {
                enabled = false;
                return;
            }
            body.centerOfMass = body.transform.InverseTransformPoint(transform.position);
        }
    }
}