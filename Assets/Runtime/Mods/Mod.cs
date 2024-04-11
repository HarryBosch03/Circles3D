using System;
using System.Collections.Generic;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Weapons;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Mods
{
    public abstract class Mod : NetworkBehaviour
    {
        [Space(20)]
        public string displayName;
        [TextArea] public string description;
        public List<StatChange> changes = new();
        public GameObject weaponAddition;

        public StatBoard statboard { get; private set; }
        public PlayerAvatar player { get; private set; }
        public Gun gun => player.gun;
        public List<Projectile> projectiles => player.gun.projectiles;

        public string identifier => ValidateIdentity(name);
        private static string ValidateIdentity(string identifier) => identifier.ToLower().Replace(" ", "");
        public bool IdentifiesAs(string identifier) => ValidateIdentity(identifier) == this.identifier;

        public string FormatName()
        {
            var input = GetType().Name;
            var name = string.Empty;

            foreach (var c in input)
            {
                if (c >= 'A' && c <= 'Z') name += ' ';
                name += c;
            }

            return name.Trim();
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = FormatName();
            }
        }

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

        protected virtual void LateUpdate()
        {
            if (weaponAddition)
            {
                if (weaponAddition.gameObject.layer != gun.modelDataActive.layer)
                {
                    foreach (var child in weaponAddition.GetComponentsInChildren<Transform>())
                    {
                        child.gameObject.layer = gun.modelDataActive.layer;
                    }
                }

                var root = gun.modelDataActive.root;
                weaponAddition.transform.position = root.position;
                weaponAddition.transform.rotation = root.rotation;
            }
        }

        public override void Spawned()
        {
            Projectile.ProjectileHitEvent += OnProjectileHit;
            name = displayName;
        }

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

        [Serializable]
        public struct StatChange
        {
            private string statName;
            private string change;
            private Polarity polarity;

            public enum Polarity
            {
                Positive,
                Negative,
                Neutral
            }
        }
    }
}