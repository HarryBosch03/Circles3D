using Runtime.Networking;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Mods
{
    public class Hoverpack : Mod
    {
        public float velocityCancel = 5f;
        public float gravityScale = 0.1f;

        private NetInput input;

        public bool active => player && !player.movement.kcc.IsGrounded && gun.aiming;
        
        public override void Apply(ref StatBoard.Stats stats)
        {
            if (active) stats.gravity *= gravityScale;
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (active) player.movement.velocity -= player.movement.velocity * Mathf.Min(1f, velocityCancel * Runner.DeltaTime);
        }
    }
}