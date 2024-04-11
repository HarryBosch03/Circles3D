using Runtime.Damage;
using Runtime.UI;
using Runtime.Util;
using UnityEngine;

namespace Runtime.Player
{
    public class PlayerUI : MonoBehaviour
    {
        public float barFullHeight = 80f;
        public float barHalfHeight = 50f;
        public new AnimationCurve animation = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public float animationSpeed = 10f;
        
        private PlayerAvatar avatar;
        private HealthController health;

        private Canvas canvas;
        private UIBar healthBar;
        private UIBar bufferBar;
        private float healthBufferBlend;

        private void Awake()
        {
            avatar = GetComponent<PlayerAvatar>();
            health = GetComponent<HealthController>();

            canvas = transform.Find<Canvas>("Overlay");
            healthBar = canvas.transform.Find<UIBar>("Vitality/Health");
            bufferBar = canvas.transform.Find<UIBar>("Vitality/Buffer");
        }

        private void Update()
        {
            if (!avatar.activeViewer)
            {
                canvas.enabled = false;
                return;
            }
            
            canvas.enabled = true;
            healthBufferBlend = Mathf.MoveTowards(healthBufferBlend, health.currentBuffer > 0 && health.maxBuffer > 0 ? 1f : 0f, animationSpeed * Time.deltaTime);
            
            var t = animation.Evaluate(healthBufferBlend);
            healthBar.rectTransform.sizeDelta = new Vector2
            {
                x = healthBar.rectTransform.sizeDelta.x,
                y = Mathf.Lerp(barFullHeight, barHalfHeight, t),
            };
            bufferBar.rectTransform.sizeDelta = new Vector2
            {
                x = bufferBar.rectTransform.sizeDelta.x,
                y = Mathf.Lerp(barHalfHeight, barFullHeight, t),
            };
            
            healthBar.SetValue(health.currentPartialHealth, health.maxHealth);
            bufferBar.SetValue(health.currentPartialBuffer, health.maxBuffer);
        }
    }
}