using System.Collections.Generic;
using Runtime.Damage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class Bomb : MonoBehaviour
    {
        public AnimationCurve damage;
        public float knockback = 1f;
        public float range = 3f;
        public int rays = 48;
        public float lifetime = 0.5f;
        public GameObject vfx;
        public float vfxLifetime;

        private float age;

        [HideInInspector] public GameObject owner;

        private void OnEnable()
        {
            if (vfx) vfx.SetActive(false);
        }

        private void FixedUpdate()
        {
            age += Time.deltaTime;
            if (age > lifetime)
            {
                Detonate();
            }
        }

        private void OnValidate()
        {
            var keys = damage.keys;
            if (keys.Length > 1)
            {
                range = keys[^1].time;
            }
        }

        private void Detonate()
        {
            var ignoreList = new List<IDamageable>();
            for (var i = 0; i < rays; i++)
            {
                var ray = new Ray(transform.position, Random.insideUnitSphere);
                if (Physics.Raycast(ray, out var hit, range))
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null && !ignoreList.Contains(damageable))
                    {
                        damageable.Damage(owner, new DamageArgs((int)damage.Evaluate(hit.distance), knockback), hit.point, ray.direction, out _);
                        ignoreList.Add(damageable);
                    }
                }
            }

            if (vfx)
            {
                vfx.SetActive(true);
                vfx.transform.SetParent(null);
                Destroy(vfx, vfxLifetime);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            for (var i = 0; i < rays; i++)
            {
                var ray = new Ray(transform.position, Random.insideUnitSphere);
                if (Physics.Raycast(ray, out var hit, range))
                {
                    Gizmos.DrawLine(ray.origin, hit.point);
                }
                else
                {
                    Gizmos.DrawLine(ray.origin, ray.GetPoint(range));
                }
            }
        }
    }
}