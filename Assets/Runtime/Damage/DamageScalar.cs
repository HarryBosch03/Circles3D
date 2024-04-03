using FishNet.Object;
using UnityEngine;

namespace Runtime.Damage
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
        
        private void Awake()
        {
            parent = transform.parent.GetComponentInParent<IHealthController>();
            if (parent == null)
            {
                Destroy(this);
                throw new System.Exception($"DamageScalar \"{name}\" component is missing parent");
            }
        }

        public void Damage(NetworkObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out IDamageable.DamageReport report)
        {
            args.damageScale *= damageScale;
            parent.Damage(invoker, args, point, velocity, out report);
        }
    }
}