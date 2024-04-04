using System;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Damage
{
    public class HealthController : MonoBehaviour, IHealthController
    {
        const float BufferToHealth = 40f;

        public float currentPartialHealth;
        public float currentPartialBuffer;
        public int maxHealth_Internal = 100;
        public int maxBuffer_Internal = 0;
        public bool invulnerable;

        [Space]
        public float regenDelay = 5f;
        public float regenHealthPerSecond = 10.0f;
        public float regenHealthToBufferExchangeRate = 40.0f;

        private StatBoard stats;

        private float lastDamageTime;
        private float regenTimer;

        public Rigidbody body { get; private set; }
        public int currentHealth => Mathf.FloorToInt(currentPartialHealth);
        public int currentBuffer => Mathf.FloorToInt(currentPartialBuffer);
        public int maxHealth => maxHealth_Internal;
        public int maxBuffer => maxBuffer_Internal;

        public static event Action<HealthController, GameObject, DamageArgs, Vector3, Vector3> DiedEvent;

        protected virtual void Awake()
        {
            body = GetComponentInParent<Rigidbody>();
            stats = GetComponentInParent<StatBoard>();
        }

        private void OnEnable()
        {
            currentPartialHealth = maxHealth_Internal;
            currentPartialBuffer = maxBuffer_Internal;
        }

        private void FixedUpdate()
        {
            if (stats)
            {
                maxHealth_Internal = stats.maxHealth.AsInt();
                maxBuffer_Internal = stats.maxBuffer.AsInt();
            }

            var healthy = currentHealth >= maxHealth && currentBuffer >= maxBuffer;
            if (!healthy)
            {
                if (regenTimer < regenDelay)
                {
                    regenTimer += Time.deltaTime;
                }
                else
                {
                    if (currentPartialHealth < maxHealth)
                    {
                        ChangeHealth(regenHealthPerSecond * Time.deltaTime);
                    }
                    else
                    {
                        ChangeBuffer(regenHealthPerSecond / regenHealthToBufferExchangeRate * Time.deltaTime);
                    }
                }
            }
            else
            {
                regenTimer = 0f;
            }
        }

        public void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out IDamageable.DamageReport report)
        {
            lastDamageTime = Time.time;

            report.victim = gameObject;
            report.finalDamage = args;
            report.lethal = false;
            
            if (currentPartialBuffer > 0)
            {
                ChangeBuffer(-1);
            }
            else
            {
                ChangeHealth(-args.damage);
            }

            if (currentPartialHealth <= 0 && currentPartialBuffer <= 0)
            {
                report.lethal = true;
                Kill(invoker, args, point, velocity);
            }
        }

        public void ChangeHealth(float offset) => SetHealth(currentPartialHealth += offset);

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
        }

        protected virtual void Kill(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (invulnerable) return;

            DiedEvent?.Invoke(this, invoker, args, point, velocity);
            gameObject.SetActive(false);
        }

        public float GetHealthFactor()
        {
            var current = currentPartialHealth + currentPartialBuffer * BufferToHealth;
            var max = maxHealth_Internal + maxBuffer_Internal * BufferToHealth;
            return current / max;
        }
    }
}