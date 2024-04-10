using System;
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

        public void SetParent(StatBoard statboard)
        {
            this.statboard = statboard;
            statboard.mods.Add(this);

            player = statboard.GetComponent<PlayerAvatar>();
            transform.SetParent(statboard.transform);
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (statboard) statboard.mods.Remove(this);
        }

        public abstract void Apply(ref StatBoard.Stats stats);
    }
}