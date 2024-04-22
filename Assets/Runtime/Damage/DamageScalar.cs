using System;
using UnityEngine;

namespace Circles3D.Runtime.Damage
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class DamageScalar : MonoBehaviour, IHealthController
    {
        public float damageScale = 1f;
        
        public IHealthController parent;

        public int currentHealth => parent.currentHealth;
        public int currentBuffer => parent.currentBuffer;
        public int maxHealth => parent.maxHealth;
        public int maxBuffer => parent.maxBuffer;
        public float GetHealthFactor() => parent.GetHealthFactor();
        public bool isSoft => parent.isSoft;
        
        public event Action HealthChangedEvent;
        public event Action<GameObject, DamageArgs, Vector3, Vector3, Vector3> damageEvent;
        
        private void Awake()
        {
            parent = transform.parent.GetComponentInParent<IHealthController>();
            if (parent == null)
            {
                Destroy(this);
                throw new System.Exception($"DamageScalar \"{name}\" component is missing parent");
            }
        }

        private void OnEnable()
        {
            parent.HealthChangedEvent += HealthChangedEvent;
            parent.damageEvent += damageEvent;
        }

        private void OnDisable()
        {
            parent.HealthChangedEvent -= HealthChangedEvent;
            parent.damageEvent -= damageEvent;
        }
        
        public void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, Vector3 normal, out IDamageable.DamageReport report)
        {
            if (!args.ignoreLocationalDamage) args.damageScale *= damageScale;
            parent.Damage(invoker, args, point, velocity, normal, out report);
        }
    }
}