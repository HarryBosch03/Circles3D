using UnityEngine;

namespace Runtime.Damage
{
    public class PlayerHealthController : HealthController
    {
        public GameObject ragdollPrefab;

        private Transform model;

        protected override void Awake()
        {
            base.Awake();
            model = transform.Find("Model");
        }

        protected override void Kill(DamageArgs args, Vector3 point, Vector3 velocity)
        {
            base.Kill(args, point, velocity);

            if (ragdollPrefab)
            {
                var ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
                LineupRagdoll(model, ragdoll.transform.Find("Model"));

                foreach (var other in ragdoll.GetComponentsInChildren<Rigidbody>())
                {
                    other.AddForce(body.velocity, ForceMode.VelocityChange);
                    other.AddForceAtPosition(velocity * args.damage * 0.0006f, point, ForceMode.Impulse);
                }
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