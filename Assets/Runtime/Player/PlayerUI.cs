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
        
        private PlayerAvatar player;
        private HealthController health;

        private Canvas canvas;
        private UIBar healthBar;
        private UIBar bufferBar;
        private float healthBufferBlend;

        public bool isOwner => player.HasInputAuthority;

        private void Awake()
        {
            player = GetComponent<PlayerAvatar>();
            health = GetComponent<HealthController>();

            canvas = transform.Find<Canvas>("Overlay");
            healthBar = canvas.transform.Find<UIBar>("Vitality/Health");
            bufferBar = canvas.transform.Find<UIBar>("Vitality/Buffer");
        }

        private void Update()
        {
            if (!isOwner)
            {
                canvas.gameObject.SetActive(false);
                return;
            }
            
            canvas.gameObject.SetActive(true);
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