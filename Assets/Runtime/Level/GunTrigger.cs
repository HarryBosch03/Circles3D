using Fusion;
using Runtime.Weapons;

namespace Runtime.Level
{
    public class GunTrigger : NetworkBehaviour
    {
        public float shootDelay;

        [Networked]
        private float shootTimer { get; set; }
        private Gun gun;

        private void Awake()
        {
            gun = GetComponentInChildren<Gun>();
        }

        public override void FixedUpdateNetwork()
        {
            shootTimer += Runner.DeltaTime;
            if (shootTimer > shootDelay)
            {
                shootTimer -= shootDelay;
                gun.Shoot();
            }
        }
    }
}