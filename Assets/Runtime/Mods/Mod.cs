using System.Collections.Generic;
using Fusion;
using Runtime.Player;
using Runtime.Stats;
using Runtime.Weapons;

namespace Runtime.Mods
{
    public abstract class Mod : NetworkBehaviour
    {
        public StatBoard statboard { get; private set; }
        public PlayerAvatar player { get; private set; }
        public Gun gun => player.gun;
        public List<Projectile> projectiles => player.gun.projectiles;

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void SetOwnerRpc(NetworkBehaviourId statboardNetId)
        {
            Runner.TryFindBehaviour(statboardNetId, out StatBoard statboard);
            if (!statboard) return;

            this.statboard = statboard;
            statboard.mods.Add(this);
            
            player = statboard.GetComponent<PlayerAvatar>();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (statboard) statboard.mods.Remove(this);   
        }
        
        public abstract void Apply(ref StatBoard.Stats stats);
    }
}