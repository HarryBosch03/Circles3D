using Runtime.Stats;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Mods
{
    public class GrenadeBullets : Mod
    {
        public GameObject bombPrefab;
        
        public override void Apply(ref StatBoard.Stats stats)
        {
            stats.bulletSpeed *= 0.4f;
            stats.damage *= 0.8f;
        }

        public override void ProjectileHit(Projectile projectile, RaycastHit hit)
        {
            Instantiate(bombPrefab, hit.point, Quaternion.identity, hit.collider.transform);
        }
    }
}