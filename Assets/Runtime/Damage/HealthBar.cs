using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Damage
{
    [RequireComponent(typeof(Image))]
    public class HealthBar : MonoBehaviour
    {
        private Image fill;
        private IHealthController target;

        private void Awake()
        {
            target = GetComponentInParent<IHealthController>();
            fill = GetComponent<Image>();
        }

        private void OnEnable()
        {
            target.HealthChangedEvent += OnHealthChanged;
        }

        private void OnDisable()
        {
            target.HealthChangedEvent -= OnHealthChanged;
        }

        private void OnHealthChanged()
        {
            fill.fillAmount = Mathf.Clamp01((float)target.currentHealth / target.maxHealth);
        }
    }
}