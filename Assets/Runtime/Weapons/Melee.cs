using System.Collections;
using System.Linq;
using Circles3D.Runtime.Animation;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Networking;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Stats;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Weapons
{
    public class Melee : NetworkBehaviour
    {
        public int damageDelayFrames = 5;
        public float useDelay;
        public UnityEngine.Animation punchAnimation;
        public ArmAnimation leftArm;

        private PlayerAvatar avatar;

        [Networked] public int timer { get; set; }
        public StatBoard.Stats stats => avatar.statboard.evaluated;

        private void Awake() { avatar = GetComponentInParent<PlayerAvatar>(); }

        public override void FixedUpdateNetwork()
        {
            if (avatar.input.buttons.IsSet(NetInput.Block) && timer > useDelay * Runner.TickRate)
            {
                Debug.Log("Punch!");
                StartCoroutine(Punch());
                timer = 0;
            }
            
            if (timer == damageDelayFrames)
            {
                DealDamage();
            }

            timer++;
        }

        private void DealDamage()
        {
            var ray = new Ray(avatar.view.position, avatar.view.forward);
            var hits = Physics.SphereCastAll(ray, 0.1f, stats.punchRange).OrderBy(e => e.distance); 
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(avatar.transform)) continue;
                if (IDamageable.Damage(avatar.gameObject, hit, new DamageArgs(stats.punchDamage, stats.punchKnockback, true), ray.direction, out var report))
                {
                    if (HasStateAuthority && avatar.owningPlayerInstance) avatar.owningPlayerInstance.OnPlayerDealtDamage(report);
                }
                break;
            }
        }

        private IEnumerator Punch()
        {
            punchAnimation.Play(PlayMode.StopAll);
            leftArm.enabled = false;
            
            yield return new WaitUntil(() => !punchAnimation.isPlaying);
            leftArm.enabled = true;
        }
    }
}