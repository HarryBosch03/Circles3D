using Circles3D.Runtime.Weapons;
using Fusion;

namespace Circles3D.Runtime.Level
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