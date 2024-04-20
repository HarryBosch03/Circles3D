using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Weapons;
using UnityEngine;

namespace Circles3D.Runtime.Mods
{
    public class GrenadeBullets : Mod
    {
        public GameObject bombPrefab;

        public override void ProjectileHit(Projectile projectile, RaycastHit hit)
        {
            if (hit.collider.gameObject.activeInHierarchy)
            {
                Instantiate(bombPrefab, hit.point, Quaternion.identity, hit.collider.transform);
            }
        }
    }
}