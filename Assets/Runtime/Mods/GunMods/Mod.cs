using System.Collections.Generic;
using Runtime.Player;
using Runtime.Stats;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Mods.GunMods
{
    public abstract class Mod : MonoBehaviour
    {
        private PlayerAvatar player;
        private Gun gun;
        private StatBoard stats;

        public List<Projectile> projectiles => gun.projectiles;

        protected virtual void Awake()
        {
            stats = GetComponentInParent<StatBoard>();
            player = GetComponentInParent<PlayerAvatar>();
            gun = player.GetComponentInChildren<Gun>();
        }

        protected virtual void OnEnable() { stats.mods.Add(this); }

        protected virtual void OnDisable() { stats.mods.Remove(this); }
        
        public abstract void Apply(StatBoard statBoard);
    }
}