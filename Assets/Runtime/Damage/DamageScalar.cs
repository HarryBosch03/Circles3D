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
        }

        private void OnValidate()
        {
            Awake();
        }

        public void Damage(DamageArgs args, Vector3 point, Vector3 direction)
        {
            args.damageScale *= damageScale;
            parent.Damage(args, point, direction);
        }
    }
}