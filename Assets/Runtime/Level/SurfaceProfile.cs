using System;
using Circles3D.Runtime.Damage;
using UnityEngine;

namespace Circles3D.Runtime.Level
{
    public class SurfaceProfile : MonoBehaviour
    {
        public ParticleSystem hitEffect;
        private IDamageable parent;

        private void Awake()
        {
            parent = GetComponentInParent<IDamageable>();
        }

        private void OnEnable()
        {
            if (parent != null) parent.damageEvent += OnDamage;
            else enabled = false;
        }

        private void OnDisable()
        {
            if (parent != null) parent.damageEvent -= OnDamage;
        }

        private void Start()
        {
            if (hitEffect)
            {
                var main = hitEffect.main;
                main.loop = false;
                main.playOnAwake = false;
            }
        }

        private void OnDamage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, Vector3 normal)
        {
            if (hitEffect)
            {
                var count = hitEffect.emission.GetBurst(0).count;
                hitEffect.transform.position = point;
                hitEffect.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(velocity, normal));
                hitEffect.Play();
            }
        }

        private void Reset()
        {
            parent = GetComponentInParent<IDamageable>();
            if (parent == null) parent = gameObject.AddComponent<DamageEvents>();
        }
    }
}