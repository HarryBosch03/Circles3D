using System.Collections.Generic;
using Fusion;
using Runtime.Player;
using Runtime.Stats;
using Runtime.Weapons;
using UnityEngine;

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
            transform.SetParent(statboard.transform);

            player = statboard.GetComponent<PlayerAvatar>();
        }

        public override void FixedUpdateNetwork()
        {
            if (gun)
            {
                foreach (var projectile in gun.projectiles)
                {
                    ProjectileTick(projectile);
                }
            }
        }

        public override void Spawned() { Projectile.ProjectileHitEvent += OnProjectileHit; }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (statboard) statboard.mods.Remove(this);
            Projectile.ProjectileHitEvent -= OnProjectileHit;
        }

        public abstract void Apply(ref StatBoard.Stats stats);


        private void OnProjectileHit(Projectile projectile, RaycastHit hit)
        {
            if (projectile.shooter == player) ProjectileHit(projectile, hit);
        }

        public virtual void ProjectileHit(Projectile projectile, RaycastHit hit) { }
        public virtual void ProjectileTick(Projectile projectile) { }
    }
}