using Runtime.Damage;
using Runtime.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Hitmarker : MonoBehaviour
    {
        public float duration;
        public AnimationCurve animationCurve;
        public float fadeout;
        public bool test;

        [Space]
        public Color defaultColor = Color.white;
        public Color criticalColor = Color.yellow;
        public Color killColor = Color.red;

        private float time;
        private CanvasGroup group;
        private Image[] components;

        private PlayerAvatar player;
        
        private void Awake()
        {
            player = GetComponentInParent<PlayerAvatar>();
            group = GetComponent<CanvasGroup>();
            components = GetComponentsInChildren<Image>();
        }

        private void OnEnable()
        {
            time = duration + fadeout;
            PlayerInstance.PlayerDealtDamageEvent += OnPlayerDealtDamage;
        }

        private void OnDisable()
        {
            PlayerInstance.PlayerDealtDamageEvent -= OnPlayerDealtDamage;
        }

        private void OnPlayerDealtDamage(PlayerInstance player, IDamageable.DamageReport report)
        {
            if (!this.player) return;
            if (player != this.player.owningPlayerInstance) return;
            if (!player.isOwner) return;

            if (report.lethal) Show(Flavour.Lethal);
            else if (report.finalDamage.damageScale > 1f) Show(Flavour.Critical);
            else Show(Flavour.Hit);
        }

        private void Update()
        {
            transform.localScale = Vector3.one * animationCurve.Evaluate(time / duration);
            group.alpha = Mathf.Clamp01(1f - (time - duration) / fadeout);
            time += Time.deltaTime;
        }

        public void Show(Flavour flavour)
        {
            time = 0f;

            var color = flavour switch
            {
                Flavour.Critical => criticalColor,
                Flavour.Lethal => killColor,
                _ => defaultColor,
            };
            foreach (var image in components)
            {
                image.color = color;
            }
        }

        private void OnValidate()
        {
            if (test)
            {
                test = false;
                time = 0f;
            }
        }

        public enum Flavour
        {
            Hit,
            Critical,
            Lethal,
        }
    }
}