using System;
using System.Collections.Generic;
using Circles3D.Runtime.Damage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Circles3D.Runtime.Weapons
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
            var broadPhase = Physics.OverlapSphere(transform.position, range);
            var valid = false;
            foreach (var other in broadPhase)
            {
                var damageable = other.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;
                valid = true;
                break;
            }

            if (valid)
            {
                var ignoreList = new List<IDamageable>();
                for (var i = 0; i < rays / 2; i++)
                {
                    var u = (i + 1f) / (rays / 2 + 1f) * 180f - 90f;
                    for (var j = 0; j < rays; j++)
                    {
                        var v = j / (float)rays * 360f;

                        var angle = Quaternion.Euler(u, v, 0f);
                        var ray = new Ray(transform.position, angle * Vector3.forward);
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
            Gizmos.color = Color.magenta;
            for (var i = 0; i < rays / 2; i++)
            {
                var u = (i + 1f) / (rays / 2 + 1f) * 180f - 90f;
                for (var j = 0; j < rays; j++)
                {
                    var v = j / (float)rays * 360f;

                    var angle = Quaternion.Euler(u, v, 0f);
                    var ray = new Ray(transform.position, angle * Vector3.forward);
                    Gizmos.DrawSphere(ray.GetPoint(range), 0.02f);
                }
            }
        }
    }
}