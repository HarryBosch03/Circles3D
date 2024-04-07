using System;
using System.Collections.Generic;
using Runtime.Damage;
using Runtime.Player;
using Runtime.Stats;
using Runtime.Util;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Gun : MonoBehaviour
    {
        public const int DefaultModelLayer = 0;
        public const int ViewportModelLayer = 7;

        public Projectile projectile;
        public float aimSpeed = 40f;
        public float aimZoom = 1f;

        [Space]
        public int currentMagazine = 3;

        [Space]
        public Vector3 translationRecoil = new Vector3(0.004f, 0, -0.01f);
        public Vector3 rotationRecoil = new Vector3(-2, 0.5f, 0.5f);

        [Space]
        public float recoilSpring = 300f;
        public float recoilDamping = 20f;

        [Space]
        public ParticleSystem flash;

        [Space]
        public TMP_Text ammoText;
        public Image reloadProgress;
        public Image attackSpeedProgress;
        public CanvasGroup dot;

        private PlayerAvatar owner;
        private Rigidbody body;
        private StatBoard stats;

        private Model modelFirstPerson;
        private Model modelThirdPerson;
        private Canvas overlay;

        private bool isFirstPerson;
        private bool isVisible;

        private Transform muzzle;

        public bool aiming { get; set; }
        public Transform projectileSpawnPoint { get; set; }
        public RecoilData recoilData { get; private set; }
        public float reloadTimer { get; private set; }
        public float aimPercent { get; private set; }
        public Transform leftHandHold { get; private set; }
        public Transform rightHandHold { get; private set; }
        public float zoom => Mathf.Lerp(1f, aimZoom, aimPercent);
        public float lastShootTime { get; private set; } = float.MinValue;
        public List<Projectile> projectiles = new();

        public Action spawnProjectileEvent;

        public void Shoot()
        {
            if (currentMagazine > 0 && Time.time - lastShootTime > 1f / stats.attackSpeed)
            {
                SpawnProjectile();
            }
        }

        public void SetFirstPerson(bool isFirstPerson) => SetVisible(isVisible, isFirstPerson);
        public void SetVisible(bool isVisible) => SetVisible(isVisible, isFirstPerson);

        public void SetVisible(bool isVisible, bool isFirstPerson)
        {
            if (isVisible == this.isVisible && isFirstPerson == this.isFirstPerson) return;

            this.isVisible = isVisible;
            this.isFirstPerson = isFirstPerson;

            UpdateModelVisibility();
        }

        private void UpdateModelVisibility()
        {
            overlay.gameObject.SetActive(isVisible && isFirstPerson);
            modelFirstPerson.ShouldRender(isVisible && isFirstPerson);
            modelThirdPerson.ShouldRender(isVisible && !isFirstPerson);
        }

        private void Awake()
        {
            owner = GetComponentInParent<PlayerAvatar>();
            body = GetComponentInParent<Rigidbody>();
            stats = GetComponentInParent<StatBoard>();

            modelFirstPerson = new Model(gameObject.Find("Model.FirstPerson"));
            modelThirdPerson = new Model(gameObject.Find("Model.ThirdPerson"));

            overlay = transform.Find<Canvas>("Overlay");
            leftHandHold = modelThirdPerson.transform.Search("HandHold.L");
            rightHandHold = modelThirdPerson.transform.Search("HandHold.R");
            muzzle = modelThirdPerson.transform.Search("Muzzle");

            if (!stats) stats = gameObject.AddComponent<StatBoard>();

            isFirstPerson = false;
            isVisible = true;
            UpdateModelVisibility();

            SetModelRenderLayer(modelFirstPerson, ViewportModelLayer);
            SetModelRenderLayer(modelThirdPerson, DefaultModelLayer);
        }

        private void SetModelRenderLayer(Model model, int layer)
        {
            foreach (var child in model.gameObject.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = layer;
            }
        }

        private void Start()
        {
            reloadTimer = stats.reloadTime;
            currentMagazine = stats.magazineSize.AsIntMax(1);
        }

        private void FixedUpdate()
        {
            projectiles.RemoveAll(e => !e);

            UpdateRecoil();
            UpdateAiming();
            Reload();
            UpdateUI();
        }

        private void UpdateAiming()
        {
            aimPercent += ((aiming ? 1f : 0f) - aimPercent) * aimSpeed * Time.deltaTime;
            aimPercent = Mathf.Clamp01(aimPercent);
        }

        private void UpdateUI()
        {
            if (ammoText) ammoText.text = $"{currentMagazine}/{stats.magazineSize}";
            if (reloadProgress)
            {
                reloadProgress.fillAmount = reloadTimer / stats.reloadTime;
                reloadProgress.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - 4f * (reloadTimer - stats.reloadTime)) * 0.1f);
            }

            if (attackSpeedProgress)
            {
                var attackTime = 1f / stats.attackSpeed;
                var t = Time.time - lastShootTime;
                attackSpeedProgress.fillAmount = t / attackTime;
                attackSpeedProgress.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - 4f * (t - attackTime)) * 0.2f);
            }

            dot.alpha = 1f - aimPercent;
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

            var damage = new DamageArgs(stats.damage.AsInt(), stats.knockback);

            args.damage = damage;
            args.speed = stats.projectileSpeed;
            args.sprayAngle = stats.spray;
            args.bounces = (int)stats.bounces;
            args.homing = stats.homing;
            args.lifetime = stats.projectileLifetime;

            return args;
        }

        private void SpawnProjectile()
        {
            spawnProjectileEvent?.Invoke();

            lastShootTime = Time.time;

            var view = projectileSpawnPoint ? projectileSpawnPoint : muzzle;
            var instance = Projectile.Spawn(projectile, owner, view.position, view.forward, GetProjectileSpawnArgs());
            if (instance)
            {
                instance.velocity += body ? body.velocity : Vector3.zero;
                projectiles.Add(instance);
            }

            currentMagazine--;
            reloadTimer = 0f;

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

        public struct RecoilData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 rotation;
            public Vector3 angularVelocity;
        }

        public class Model
        {
            public readonly GameObject gameObject;
            public readonly Transform transform;
            public readonly Renderer[] renderers;

            public void ShouldRender(bool state)
            {
                foreach (var r in renderers) r.enabled = state;
            }

            public Model(GameObject gameObject)
            {
                this.gameObject = gameObject;

                transform = gameObject.transform;
                renderers = gameObject.GetComponentsInChildren<Renderer>();
            }
        }
    }
}