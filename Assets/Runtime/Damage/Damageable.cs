using FishNet.Object;
using UnityEngine;

namespace Runtime.Damage
{
    public interface IDamageable
    {
        public void Damage(NetworkObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out DamageReport report);
        
        public static bool Damage(NetworkObject invoker, RaycastHit hit, DamageArgs args, Vector3 velocity, out DamageReport report)
        {
            report = new DamageReport();
            
            var body = hit.collider.attachedRigidbody;
            if (body)
            {
                body.AddForce(velocity * args.knockback);
            }
            
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage(invoker, args, hit.point, velocity, out report);
                return true;
            }
            return false;
        }

        public struct DamageReport
        {
            public NetworkObject victim;
            public DamageArgs finalDamage;
            public bool lethal;
        }
    }
}