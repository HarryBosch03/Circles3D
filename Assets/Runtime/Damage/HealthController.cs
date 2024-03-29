using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Runtime.Damage
{
    public class HealthController : NetworkBehaviour, IHealthController
    {
        [SyncVar]
        public int currentHealth;
        [SyncVar]
        public int currentBuffer;
        [SyncVar]
        public int maxHealth = 100;
        [SyncVar]
        public int maxBuffer = 0;
        public bool invulnerable;

        [Space]
        public float regenDelay;
        public float regenRate;
        public int regenAmount;

        private Rigidbody body;
        
        private float lastDamageTime;
        private float regenTimer;
        
        private void Awake() { body = GetComponentInParent<Rigidbody>(); }

        private void OnEnable()
        {
            currentHealth = maxHealth;
            currentBuffer = maxBuffer;
        }

        private void FixedUpdate()
        {
            if (Time.time - lastDamageTime > regenDelay && currentHealth < maxHealth)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer > 1f / regenRate)
                {
                    if (currentHealth < maxHealth)
                    {
                        SetHealth(currentHealth + regenAmount);
                    }
                    else if (currentBuffer < maxBuffer)
                    {
                        SetBuffer(currentBuffer + 1);
                    }

                    regenTimer -= 1f / regenRate;
                }
            }
            else
            {
                regenTimer = 0f;
            }
        }

        public void Damage(DamageArgs args, Vector3 point, Vector3 velocity)
        {
            args.damage = Mathf.Max(1, args.damage);
            lastDamageTime = Time.time;

            if (body) body.AddForceAtPosition(velocity.normalized * args.GetKnockback(velocity.magnitude), point, ForceMode.Impulse);

            if (IsServer)
            {
                if (currentBuffer > 0)
                {
                    SetBuffer(currentBuffer - 1);
                }
                else
                {
                    SetHealth(currentHealth - args.damage);
                }
            }

            if (currentHealth < 0 && currentBuffer < 0)
            {
                Kill(args, point, velocity);
            }
        }

        public void SetHealth(int health)
        {
            if (!IsServer) return;
            
            currentHealth = health;
            HealthChanged();
        }

        public void SetBuffer(int buffer)
        {
            if (!IsServer) return;
            
            currentBuffer = buffer;
            HealthChanged();
        }
        
        public void HealthChanged()
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentBuffer = Mathf.Min(currentBuffer, maxBuffer);
        }

        private void Kill(DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (invulnerable) return;
            gameObject.SetActive(false);
        }

        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public int GetCurrentBuffer() => currentBuffer;
        public int GetMaxBuffer() => maxBuffer;
    }
}