using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Weapons;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Mods
{
    public class Mod : NetworkBehaviour
    {
        [Space(20)]
        public string displayName;
        public string description;
        public List<StatChange> changes = new();
        public GameObject weaponAddition;
        
        public StatBoard statboard { get; private set; }
        public PlayerAvatar player { get; private set; }
        public Gun gun => player.gun;
        public List<Projectile> projectiles => player.gun.projectiles;

        public string identifier => ValidateIdentity(name);
        private static string ValidateIdentity(string identifier) => identifier.ToLower().Replace(" ", "");
        public bool IdentifiesAs(string identifier) => ValidateIdentity(identifier) == this.identifier;

        protected virtual void Awake()
        {
            foreach (var change in changes)
            {
                change.Validate();
            }
        }


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

            var order = StatBoard.Stats.Metadata.Keys.ToList();
            changes = changes.OrderBy(e => order.IndexOf(e.fieldName)).ToList();
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

        public virtual void Apply(ref StatBoard.Stats stats)
        {
            foreach (var change in changes)
            {
                change.Apply(ref stats);
            }
        }

        private void OnProjectileHit(Projectile projectile, RaycastHit hit)
        {
            if (projectile.shooter == player) ProjectileHit(projectile, hit);
        }

        public virtual void ProjectileHit(Projectile projectile, RaycastHit hit) { }
        public virtual void ProjectileTick(Projectile projectile) { }

        public string GetChangeList()
        {
            var str = "";
            foreach (var change in changes)
            {
                str += $"{change}\n";
            }

            return str;
        }
        
        [Serializable]
        public struct StatChange
        {
            public string fieldName;
            public ChangeType changeType;
            public float value;
            
            public StatBoard.Stats.StatMetadata metadata;
            public FieldInfo field;

            public StatChange(string fieldName, ChangeType changeType, float value) : this()
            {
                this.fieldName = fieldName;
                this.changeType = changeType;
                this.value = value;
            }

            public void Validate()
            {
                metadata = StatBoard.Stats.GetMetadata(fieldName);
                field = GetField(fieldName);
            }

            private static FieldInfo GetField(string fieldName) => typeof(StatBoard.Stats).GetField(fieldName);

            public void Apply(ref StatBoard.Stats stats)
            {
                if (field == null)
                {
                    throw new Exception($"Could not apply stat change to \"{fieldName}\", FieldInfo is null");
                }
                
                var value = (float)field.GetValue(stats);
                switch (changeType)
                {
                    case ChangeType.Percentage:
                        value += value * this.value;
                        break;
                    case ChangeType.Constant:
                        value += this.value;
                        break;
                }

                field.SetValue(stats, value);
            }

            public enum ChangeType
            {
                Percentage,
                Constant,
            }

            public override string ToString()
            {
                var name = StatBoard.Stats.GetMetadata(fieldName);
                if (name == null) return null;

                return changeType switch
                {
                    ChangeType.Percentage => $"{(value > 0 ? "+" : "")}{value:P0}% {name}",
                    ChangeType.Constant => $"{(value > 0 ? "+" : "")}{value:G0} {name}",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}