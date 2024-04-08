using System.Text.RegularExpressions;
using Fusion;
using Runtime.Player;
using Runtime.RenderFeatures;
using Runtime.Util;
using UnityEngine;

namespace Runtime.Damage
{
    public class PlayerHealthController : HealthController
    {
        [Space]
        public GameObject ragdollPrefab;
        public AnimationCurve damageFlinch;
        public AnimationCurve damageOverlayOpacity;

        private Transform model;
        private float hurtAnimationTime = float.MaxValue;
        private PlayerAvatar player;

        public GameObject latestRagdoll { get; private set; }
        public Transform latestRagdollHead { get; private set; }

        public string reasonForDeath { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            model = transform.Find("Model");
            player = GetComponent<PlayerAvatar>();
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                player.movement.cameraDutch = damageFlinch.Evaluate(hurtAnimationTime);
                hurtAnimationTime += Time.deltaTime;

                HurtOverlayFeature.Weight = 1f - GetHealthFactor();
            }
        }

        private void OnGUI()
        {
            var color = new Color(1f, 0f, 0f, damageOverlayOpacity.Evaluate(hurtAnimationTime));
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, color, Vector4.zero, Vector4.zero);
        }

        public override void Damage(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity, out IDamageable.DamageReport report)
        {
            base.Damage(invoker, args, point, velocity, out report);
            if (HasStateAuthority) RpcNotifyDamage(report);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void RpcNotifyDamage(IDamageable.DamageReport report) => hurtAnimationTime = 0f;

        protected override void Kill(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            var killerAvatar = invoker ? invoker.GetComponent<PlayerAvatar>() : null;
            var killer = killerAvatar ? killerAvatar.owningPlayerInstance : null;
            reasonForDeath = killer ? $"Killed by {killer.displayName}" : null;

            if (HasStateAuthority) SpawnRagdollRpc(velocity * args.damage * 0.0006f, point);

            base.Kill(invoker, args, point, velocity);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void SpawnRagdollRpc(Vector3 force, Vector3 point)
        {
            if (!ragdollPrefab) return;

            latestRagdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            LineupRagdoll(model, latestRagdoll.transform.Find("Model"));

            foreach (var other in latestRagdoll.GetComponentsInChildren<Rigidbody>())
            {
                other.AddForce(body.velocity * body.mass, ForceMode.VelocityChange);
                other.AddForceAtPosition(force, point, ForceMode.Impulse);
            }

            latestRagdollHead = latestRagdoll.transform.Search(@".*head.*");
        }

        private void LineupRagdoll(Transform from, Transform to)
        {
            var fromGroup = from.GetComponentsInChildren<Transform>();
            var toGroup = to.GetComponentsInChildren<Transform>();
            var length = Mathf.Min(fromGroup.Length, toGroup.Length);

            for (var i = 0; i < length; i++)
            {
                toGroup[i].localPosition = fromGroup[i].localPosition;
                toGroup[i].localRotation = fromGroup[i].localRotation;
            }
        }
    }
}