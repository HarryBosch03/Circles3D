using System;
using Circles3D.Runtime.Level;
using Circles3D.Runtime.Player;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Damage
{
    public interface IDamageable
    {
        bool isSoft { get; }
        public void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, Vector3 normal, out DamageReport report);

        public event Action<GameObject, DamageArgs, Vector3, Vector3, Vector3> damageEvent;
        
        public static bool Damage(GameObject invoker, RaycastHit hit, DamageArgs args, Vector3 velocity, out DamageReport report)
        {
            report = new DamageReport();
            
            var body = hit.collider.attachedRigidbody;
            if (body)
            {
                body.AddForce(velocity * args.knockback);
            }

            var player = hit.collider.GetComponentInParent<PlayerAvatar>();
            if (player)
            {
                player.movement.velocity += velocity * args.knockback / player.mass;
            }
            
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage(invoker, args, hit.point, velocity, hit.normal, out report);
                return damageable.isSoft;
            }
            return false;
        }

        public struct DamageReport : INetworkStruct
        {
            public static readonly DamageReport Failed = new DamageReport()
            {
                failed = true,
            };
            
            public bool failed;
            
            public NetworkId victim;
            public DamageArgs finalDamage;
            public bool lethal;
        }
    }
}