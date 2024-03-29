using System;
using FishNet.Object;
using Runtime.Damage;
using Runtime.Player;
using Runtime.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GunStatBoard))]
    public class Gun : NetworkBehaviour
    {
        public Projectile projectile;
        public float aimSpeed = 40f;

        [Space]
        public int currentMagazine = 3;

        [Space]
        public Vector3 translationRecoil = new Vector3(0.004f, 0, -0.01f);
        public Vector3 rotationRecoil = new Vector3(-2, 0.5f, 0.5f);

        [Space]
        public float recoilSpring = 300f;
        public float recoilDamping = 20f;
        public float recoilFollowThrough = 125f;

        [Space]
        public ParticleSystem flash;

        [Space]
        public TMP_Text ammoText;
        public Image reloadProgress;
        public Image attackSpeedProgress;

        private GunStatBoard stats;
        private float shootTimer;

        public PlayerAvatar player { get; private set; }
        public RecoilData recoilData { get; private set; }
        public float reloadTimer { get; private set; }
        public float aimPercent { get; private set; }
        public float lastShootTime { get; private set; } = float.MinValue;

        public Action<float> spawnProjectileEvent;

        private void Awake()
        {
            player = GetComponentInParent<PlayerAvatar>();
            
            stats = GetComponent<GunStatBoard>();
        }

        private void Start()
        {
            reloadTimer = stats.reloadTime;
            currentMagazine = stats.magazineSize.AsIntMax(1);
        }

        private void LateUpdate()
        {
            if (IsOwner)
            {
                aimPercent += ((Mouse.current.rightButton.isPressed ? 1f : 0f) - aimPercent) * aimSpeed * Time.deltaTime;
                aimPercent = Mathf.Clamp01(aimPercent);
            }

            player.orientation += new Vector2(recoilData.velocity.x, recoilData.velocity.z) * Time.deltaTime * recoilFollowThrough;
        }

        private void FixedUpdate()
        {
            if (IsOwner)
            {
                if (Mouse.current.leftButton.isPressed && currentMagazine > 0) shootTimer += Time.deltaTime;
                else shootTimer = 0f;
            }

            var i = 0;
            while (shootTimer > 0f)
            {
                SpawnProjectile(shootTimer);
                SpawnProjectileOnServer(shootTimer);
                shootTimer -= stats.attackSpeed;
                if (i++ > 50) break;
            }

            UpdateRecoil();
            Reload();

            UpdateUI();

            PackNetworkData();
        }

        private void UpdateUI()
        {
            if (ammoText) ammoText.text = $"{currentMagazine}/{stats.magazineSize}";
            if (reloadProgress)
            {
                reloadProgress.fillAmount = reloadTimer / stats.reloadTime;
                reloadProgress.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - 4f * (reloadTimer - stats.reloadTime)) * 0.04f);
            }

            if (attackSpeedProgress)
            {
                var shootTime = stats.attackSpeed;
                var t = Time.time - lastShootTime;
                attackSpeedProgress.fillAmount = t / shootTime;
                attackSpeedProgress.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - 4f * (t - shootTime)) * 0.04f);
            }
        }

        private void Reload()
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer > stats.reloadTime)
            {
                currentMagazine = stats.magazineSize.AsIntMax(1);
            }
        }

        private void UpdateRecoil()
        {
            var recoilData = this.recoilData;

            recoilData.position += recoilData.velocity * Time.deltaTime;
            recoilData.velocity -= (recoilData.position * recoilSpring + recoilData.velocity * recoilDamping) * Time.deltaTime;

            recoilData.rotation += recoilData.angularVelocity * Time.deltaTime;
            recoilData.angularVelocity -= (recoilData.rotation * recoilSpring + recoilData.angularVelocity * recoilDamping) * Time.deltaTime;

            this.recoilData = recoilData;
        }

        private Projectile.SpawnArgs GetProjectileSpawnArgs()
        {
            Projectile.SpawnArgs args;
            
            DamageArgs damage;
            damage.damage = stats.damage.AsInt();
            damage.baseKnockback = stats.knockback;

            args.damage = damage;
            args.speed = stats.projectileSpeed;
            args.sprayAngle = stats.spray;
            
            return args;
        }
        
        private void SpawnProjectile(float subTime)
        {
            spawnProjectileEvent?.Invoke(subTime);

            lastShootTime = Time.time - subTime;

            var view = player.view;
            var instance = Projectile.Spawn(projectile, view.position, view.forward, GetProjectileSpawnArgs());

            currentMagazine--;
            reloadTimer = 0f;

            ApplyRecoilForceToPlayer(instance);
            
            var recoilData = this.recoilData;
            recoilData.position += new Vector3
            {
                x = translationRecoil.x * Mathf.Sign(Random.value - 0.5f),
                y = translationRecoil.y,
                z = translationRecoil.z,
            } * stats.recoil;
            recoilData.rotation += new Vector3
            {
                x = rotationRecoil.x,
                y = rotationRecoil.y * Mathf.Sign(Random.value - 0.5f),
                z = rotationRecoil.z * Mathf.Sign(Random.value - 0.5f),
            } * stats.recoil;
            this.recoilData = recoilData;

            if (flash) flash.Play();
        }

        private void ApplyRecoilForceToPlayer(Projectile projectile)
        {
            var args = GetProjectileSpawnArgs();
            var force = args.damage.GetKnockback(args.speed) * 0.5f;
            player.body.AddForce(-projectile.transform.forward * force, ForceMode.Impulse);
        }

        [ServerRpc]
        private void SpawnProjectileOnServer(float subTime)
        {
            SpawnProjectileOnClients(subTime);
            if (!IsOwner) SpawnProjectile(subTime);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void SpawnProjectileOnClients(float subTime)
        {
            if (!IsOwner) SpawnProjectile(subTime);
        }

        private void PackNetworkData()
        {
            if (!IsOwner) return;

            NetworkData data;
            data.aimPercent = aimPercent;

            SendDataToServer(data);
        }

        [ServerRpc]
        private void SendDataToServer(NetworkData data)
        {
            UnpackNetworkData(data);
            SendDataToClient(data);
        }

        [ObserversRpc]
        private void SendDataToClient(NetworkData data) { UnpackNetworkData(data); }

        private void UnpackNetworkData(NetworkData data)
        {
            if (IsOwner) return;

            aimPercent = data.aimPercent;
        }

        public struct NetworkData
        {
            public float aimPercent;
        }

        public struct RecoilData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 rotation;
            public Vector3 angularVelocity;
        }
    }
}