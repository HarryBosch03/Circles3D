using UnityEngine;

namespace Runtime.Damage
{
    public interface IDamageable
    {
        public void Damage(DamageArgs args, Vector3 point, Vector3 velocity);
        
        public static bool Damage(RaycastHit hit, DamageArgs args, Vector3 velocity)
        {
            var body = hit.collider.attachedRigidbody;
            if (body)
            {
                body.AddForce(velocity * args.knockback);
            }
            
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage(args, hit.point, velocity);
                return true;
            }
            return false;
        }
    }
}