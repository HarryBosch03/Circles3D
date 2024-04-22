using System;
using Circles3D.Runtime.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Circles3D.Runtime.Damage
{
    public class HealthBar : MonoBehaviour
    {
        public GameObject health;
        public Image healthFill;
        public GameObject buffer;
        public Image bufferFill;
        
        private PlayerAvatar avatar;
        private IHealthController target;
        private Camera mainCamera;

        private void Awake()
        {
            avatar = GetComponentInParent<PlayerAvatar>();
            target = GetComponentInParent<IHealthController>();
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            target.HealthChangedEvent += OnHealthChanged;
        }

        private void OnDisable()
        {
            target.HealthChangedEvent -= OnHealthChanged;
        }

        private void LateUpdate()
        {
            var vec = mainCamera.transform.position - transform.position;
            vec.y = 0f;
            vec = vec.normalized;

            if (vec == Vector3.zero) return;
            
            transform.rotation = Quaternion.LookRotation(vec);
        }

        private void OnHealthChanged()
        {
            health.SetActive(!avatar.activeViewer && avatar.health.alive && target.maxHealth > 0);
            buffer.SetActive(!avatar.activeViewer && avatar.health.alive && target.maxBuffer > 0);
            
            if (health.activeSelf) SetFill(healthFill, (float)target.currentHealth / target.maxHealth);
            if (buffer.activeSelf) SetFill(bufferFill, (float)target.currentBuffer / target.currentHealth);
        }

        private void SetFill(Image image, float percent)
        {
            if (!float.IsFinite(percent)) percent = 0f;
            image.rectTransform.localScale = new Vector3(Mathf.Clamp01(percent), 1f, 1f);
        }
    }
}