using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Weapons;
using UnityEngine;

namespace Circles3D.Runtime.Mods
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