using Runtime.Networking;
using Runtime.Stats;
using UnityEngine;

namespace Runtime.Mods
{
    public class Hoverpack : Mod
    {
        public float strength = 5f;

        private NetInput input;
        public bool active => player && !player.movement.onGround && player.gun.aiming;
        
        public override void Apply(ref StatBoard.Stats stats) { }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (active) player.movement.velocity -= player.movement.velocity * Mathf.Min(1f, strength * Runner.DeltaTime);
        }
    }
}