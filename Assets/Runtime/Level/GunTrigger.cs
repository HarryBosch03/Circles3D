using FishNet.Object;
using Runtime.Weapons;
using UnityEngine;

public class GunTrigger : NetworkBehaviour
{
    public float shootDelay;

    private float shootTimer;
    private Gun gun;

    private void Awake()
    {
        gun = GetComponentInChildren<Gun>();
    }

    private void FixedUpdate()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer > shootDelay)
        {
            shootTimer -= shootDelay;
            gun.Shoot();
        }
    }
}