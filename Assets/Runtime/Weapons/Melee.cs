using Circles3D.Runtime.Networking;
using Circles3D.Runtime.Player;
using Fusion;

namespace Circles3D.Runtime.Weapons
{
    public class Melee : NetworkBehaviour
    {
        public float useDelay;

        private PlayerAvatar avatar;

        [Networked] public float timer { get; set; }

        private void Awake() { avatar = GetComponentInParent<PlayerAvatar>(); }

        public override void FixedUpdateNetwork()
        {
            if (avatar.input.buttons.IsSet(NetInput.Block) && timer > useDelay)
            {
                timer = 0;
            }

            timer += Runner.DeltaTime;
        }
    }
}