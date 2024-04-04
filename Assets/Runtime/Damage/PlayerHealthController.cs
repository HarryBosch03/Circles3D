using Runtime.Player;
using UnityEngine;

namespace Runtime.Damage
{
    public class PlayerHealthController : HealthController
    {
        public GameObject ragdollPrefab;

        private Transform model;
        
        public string reasonForDeath { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            model = transform.Find("Model");
        }

        protected override void Kill(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            var killerAvatar = invoker ? invoker.GetComponent<PlayerAvatar>() : null;
            var killer = killerAvatar ? killerAvatar.owningPlayerInstance : null;
            reasonForDeath = killer ? $"Killed by {killer.displayName}" : null;
            
            SpawnRagdoll(velocity * args.damage * 0.0006f, point);
            
            base.Kill(invoker, args, point, velocity);
        }
        
        private void SpawnRagdoll(Vector3 force, Vector3 point)
        {
            if (!ragdollPrefab) return;
            
            var ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            LineupRagdoll(model, ragdoll.transform.Find("Model"));

            foreach (var other in ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                other.AddForce(body.velocity, ForceMode.VelocityChange);
                other.AddForceAtPosition(force, point, ForceMode.Impulse);
            }
        }

        private void LineupRagdoll(Transform from, Transform to)
        {
            var fromGroup = from.GetComponentsInChildren<Transform>();
            var toGroup = to.GetComponentsInChildren<Transform>();
            var length = Mathf.Min(fromGroup.Length, toGroup.Length);

            for (var i = 0; i < length; i++)
            {
                toGroup[i].localPosition = fromGroup[i].localPosition;
                toGroup[i].localRotation = fromGroup[i].localRotation;
            }
        }
    }
}