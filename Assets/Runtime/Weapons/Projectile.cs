using System.Linq;
using Runtime.Damage;
using UnityEngine;

namespace Runtime.Weapons
{
    public class Projectile : MonoBehaviour
    {
        public GameObject hitFX;
        
        private SpawnArgs args;
        public Vector3 velocity;
        private bool dead;

        public static Projectile Spawn(Projectile prefab, Vector3 position, Vector3 direction, SpawnArgs args)
        {
            direction.Normalize();
            var orientation = Quaternion.LookRotation(direction);

            var pa = Random.Range(-Mathf.PI, Mathf.PI);
            var pd = Random.Range(0f, args.sprayAngle);
            
            orientation *= Quaternion.Euler(new Vector3(Mathf.Cos(pa), Mathf.Sin(pa)) * pd);
            direction = orientation * Vector3.forward;
            
            var instance = Instantiate(prefab, position, Quaternion.LookRotation(direction));
            instance.args = args;
            instance.velocity = direction * args.speed;
            return instance;
        }

        private void FixedUpdate()
        {
            Collide();
            
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;
        }

        private void Collide()
        {
            var ray = new Ray(transform.position, velocity);
            var step = velocity.magnitude * Time.deltaTime * 1.01f;

            var collisions = Physics.RaycastAll(ray, step).OrderBy(e => e.distance);

            foreach (var hit in collisions)
            {
                ProcessHit(hit);
                if (dead) break;
            }
        }

        private void ProcessHit(RaycastHit hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.Damage(args.damage, hit.point, velocity);
            }
            
            if (hitFX) Instantiate(hitFX, hit.point, Quaternion.LookRotation(hit.normal));
            dead = true;
            Destroy(gameObject);
        }

        [System.Serializable]
        public struct SpawnArgs
        {
            public DamageArgs damage;
            public float speed;
            public float sprayAngle;
        }
    }
}
