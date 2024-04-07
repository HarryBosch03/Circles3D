using Fusion;
using Runtime.Damage;
using Runtime.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Hitmarker : NetworkBehaviour
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
            if (!HasStateAuthority) return;
            if (report.failed) return;
            
            if (report.lethal) ShowRpc(Flavour.Lethal);
            else if (report.finalDamage.damageScale > 1f) ShowRpc(Flavour.Critical);
            else ShowRpc(Flavour.Hit);
        }

        private void Update()
        {
            transform.localScale = Vector3.one * animationCurve.Evaluate(time / duration);
            group.alpha = Mathf.Clamp01(1f - (time - duration) / fadeout);
            time += Time.deltaTime;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void ShowRpc(Flavour flavour)
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