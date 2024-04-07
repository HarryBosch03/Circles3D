using System;
using Fusion;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Damage
{
    [RequireComponent(typeof(NetworkObject))]
    public class HealthController : NetworkBehaviour, IHealthController
    {
        const float BufferToHealth = 40f;

        public int maxHealth_Internal = 100;
        public int maxBuffer_Internal = 0;
        public bool invulnerable;

        [Space]
        public float regenDelay = 5f;
        public float regenHealthPerSecond = 10.0f;
        public float regenHealthToBufferExchangeRate = 40.0f;

        private StatBoard stats;

        [Networked]
        private float regenTimer { get; set; }

        public Rigidbody body { get; private set; }

        public int currentHealth => Mathf.FloorToInt(currentPartialHealth);
        public int currentBuffer => Mathf.FloorToInt(currentPartialBuffer);
        [Networked] public float currentPartialHealth { get; private set; }
        [Networked] public float currentPartialBuffer { get; private set; }
        [Networked] public int maxHealth { get; private set; }
        [Networked] public int maxBuffer { get; private set; }
        [Networked] public bool alive { get; private set; }

        public event Action HealthChangedEvent;
        
        public event Action<GameObject, DamageArgs, Vector3, Vector3> DiedEvent;

        protected virtual void Awake()
        {
            body = GetComponentInParent<Rigidbody>();
            stats = GetComponentInParent<StatBoard>();
        }

        public override void Spawned()
        {
            UpdateFromStats();
            
            currentPartialHealth = maxHealth;
            currentPartialBuffer = maxBuffer;
            HealthChanged();
        }

        public override void FixedUpdateNetwork()
        {
            UpdateFromStats();

            var healthy = currentHealth >= maxHealth && currentBuffer >= maxBuffer;
            if (!healthy && alive)
            {
                if (regenTimer < regenDelay)
                {
                    regenTimer += Runner.DeltaTime;
                }
                else
                {
                    if (currentHealth < maxHealth)
                    {
                        ChangeHealth(regenHealthPerSecond * Runner.DeltaTime);
                    }
                    else
                    {
                        ChangeBuffer(regenHealthPerSecond / regenHealthToBufferExchangeRate * Runner.DeltaTime);
                    }
                }
            }
            else
            {
                regenTimer = 0f;
            }
            
            HealthChanged();
        }

        private void UpdateFromStats()
        {
            if (stats)
            {
                maxHealth = stats.maxHealth.AsInt();
                maxBuffer = stats.maxBuffer.AsInt();
            }
            else
            {
                maxHealth = maxHealth_Internal;
                maxBuffer = maxBuffer_Internal;
            }
        }

        public void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out IDamageable.DamageReport report)
        {
            report = default;
            if (!alive) return;
            
            regenTimer = 0f;

            report.victim = gameObject;
            report.finalDamage = args;
            report.lethal = false;

            if (currentBuffer > 0) ChangeBuffer(-1);
            else ChangeHealth(-args.damage);

            if (currentHealth <= 0 && currentBuffer <= 0)
            {
                report.lethal = true;
                Kill(invoker, args, point, velocity);
            }
        }

        public void ChangeHealth(float offset) => SetHealth(currentPartialHealth + offset);

        public void SetHealth(float health)
        {
            currentPartialHealth = health;
            HealthChanged();
        }

        public void ChangeBuffer(float increment) => SetBuffer(currentPartialBuffer + increment);

        public void SetBuffer(float buffer)
        {
            currentPartialBuffer = buffer;
            HealthChanged();
        }

        public void HealthChanged()
        {
            currentPartialHealth = Mathf.Min(currentPartialHealth, maxHealth);
            currentPartialBuffer = Mathf.Min(currentPartialBuffer, maxBuffer);
            
            HealthChangedEvent?.Invoke();
        }

        protected virtual void Kill(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (invulnerable || !alive) return;

            DiedEvent?.Invoke(invoker, args, point, velocity);

            alive = false;
        }

        public float GetHealthFactor()
        {
            var current = currentHealth + currentBuffer * BufferToHealth;
            var max = maxHealth_Internal + maxBuffer_Internal * BufferToHealth;
            return current / max;
        }
        
        
        public void Spawn()
        {
            UpdateFromStats();
            alive = true;
            currentPartialHealth = maxHealth;
            currentPartialBuffer = maxBuffer;
            regenTimer = 0f;
        }
    }
}