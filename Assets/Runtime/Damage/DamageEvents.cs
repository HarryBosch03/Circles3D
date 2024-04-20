using System;
using UnityEngine;

namespace Circles3D.Runtime.Damage
{
    public class DamageEvents : MonoBehaviour, IDamageable
    {
        public bool isSoft => false;

        public event Action<GameObject, DamageArgs, Vector3, Vector3, Vector3> damageEvent;

        public void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, Vector3 normal, out IDamageable.DamageReport report)
        {
            damageEvent?.Invoke(invoker, args, point, velocity, normal);
            report = IDamageable.DamageReport.Failed;
        }
    }
}