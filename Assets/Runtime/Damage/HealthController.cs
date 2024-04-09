using System;
using Fusion;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Damage
{
    [RequireComponent(typeof(NetworkObject))]
    public class HealthController : NetworkBehaviour, IHealthController
    {
        public const float BufferToHealth = 30f;

        public int maxHealth_Internal = 100;
        public int maxBuffer_Internal = 0;
        public bool invulnerable;

        [Space]
        public float regenDelay = 5f;
        public float regenHealthPerSecond = 10.0f;
        public float regenHealthToBufferExchangeRate = 40.0f;

        private StatBoard statboard;

        [Networked]
        private float regenTimer { get; set; }

        public Rigidbody body { get; private set; }

        public StatBoard.Stats stats => statboard.evaluated;
        public int currentHealth => Mathf.FloorToInt(currentPartialHealth);
        public int currentBuffer => Mathf.FloorToInt(currentPartialBuffer);
        [Networked] public float currentPartialHealth { get; private set; }
        [Networked] public float currentPartialBuffer { get; private set; }
        [Networked] public int maxHealth { get; private set; } 
        [Networked] public int maxBuffer { get; private set; }
        [Networked] public NetworkBool alive { get; private set; }

        public event Action HealthChangedEvent;
        
        public event Action<GameObject, DamageArgs, Vector3, Vector3> DiedEvent;

        protected virtual void Awake()
        {
            body = GetComponentInParent<Rigidbody>();
            statboard = GetComponentInParent<StatBoard>();
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
            if (statboard)
            {
                maxHealth = stats.maxHealth;
                maxBuffer = stats.maxBuffer;
            }
            else
            {
                maxHealth = maxHealth_Internal;
                maxBuffer = maxBuffer_Internal;
            }
        }

        public virtual void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out IDamageable.DamageReport report)
        {
            report = IDamageable.DamageReport.Failed;
            if (!alive) return;
            
            regenTimer = 0f;

            report.victim = Object;
            report.finalDamage = args;
            report.lethal = false;
            report.failed = false;

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
            if (invulnerable) return;
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