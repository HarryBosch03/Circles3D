using System;
using System.Collections.Generic;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Util;
using FMOD.Studio;
using FMODUnity;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Circles3D.Runtime.Weapons
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Gun : MonoBehaviour
    {
        public const int DefaultModelLayer = 0;
        public const int ViewportModelLayer = 7;

        public Projectile projectile;
        public float aimSpeed = 40f;
        public float baseZoom = 1f;
        public Sight currentSight;

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
        public EventReference shootSound;

        [Space]
        public TMP_Text ammoText;
        public Image reloadProgress;
        public Image attackSpeedProgress;
        public CanvasGroup crosshair;
        public float crosshairBloom = 70f;
        public float crosshairDeviance = 70f;
        
        private PlayerAvatar owner;
        private bool hasDryFired;
        private StatBoard statboard;

        private Canvas overlay;

        private bool isFirstPerson;
        private bool isVisible;
        private EventInstance[] shootEventBuffer = new EventInstance[10];
        private int shootEventBufferIndex;

        public float aimZoom => currentSight ? currentSight.zoomLevel : baseZoom;
        public StatBoard.Stats stats => statboard.evaluated;
        public Transform muzzle => modelDataFirstPerson.muzzle;
        public bool aiming { get; set; }
        public Transform projectileSpawnPoint { get; set; }
        public RecoilData recoilData { get; private set; }
        public float reloadTimer { get; private set; }
        public float aimPercent { get; private set; }
        public float zoom => Mathf.Lerp(1f, aimZoom, aimPercent);
        public float lastShootTime { get; private set; } = float.MinValue;
        public float lastInputTime { get; private set; } = float.MinValue;
        public ModelData modelDataFirstPerson { get; private set; }
        public ModelData modelDataThirdPerson { get; private set; }
        public ModelData modelDataActive => isFirstPerson ? modelDataFirstPerson : modelDataThirdPerson;
        public List<Projectile> projectiles = new();

        public Action spawnProjectileEvent;
        
        public void Shoot()
        {
            if (Time.time - lastInputTime > 1f / stats.attackSpeed)
            {
                PlayShootSound();
                if (currentMagazine > 0) SpawnProjectile();
                lastInputTime = Time.time;
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
            modelDataFirstPerson.ShouldRender(isVisible && isFirstPerson);
            modelDataThirdPerson.ShouldRender(isVisible && !isFirstPerson);
        }

        private void Awake()
        {
            owner = GetComponentInParent<PlayerAvatar>();
            GetComponentInParent<Rigidbody>();
            statboard = GetComponentInParent<StatBoard>();

            modelDataFirstPerson = new ModelData(gameObject.Find("Model.FirstPerson"), ViewportModelLayer);
            modelDataThirdPerson = new ModelData(gameObject.Find("Model.ThirdPerson"), DefaultModelLayer);

            overlay = transform.Find<Canvas>("Overlay");

            if (!statboard) statboard = gameObject.AddComponent<StatBoard>();

            isFirstPerson = false;
            isVisible = true;
            UpdateModelVisibility();

            for (var i = 0; i < shootEventBuffer.Length; i++)
            {
                shootEventBuffer[i] = RuntimeManager.CreateInstance(shootSound);
            }
        }

        private void OnDestroy()
        {
            for (var i = 0; i < shootEventBuffer.Length; i++)
            {
                shootEventBuffer[i].release();
            }
        }

        private void Start()
        {
            reloadTimer = stats.reloadTime;
            currentMagazine = stats.magazineSize;
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

            crosshair.alpha = 1f - aimPercent;
            var size = Vector2.one * 15.2f;
            size *= Vector2.one * (1f + Vector3.ProjectOnPlane(muzzle.forward, transform.forward).magnitude * crosshairBloom);
            ((RectTransform)crosshair.transform).sizeDelta = size;
            var position = Vector2.zero;
            position += new Vector2(Vector3.Dot(muzzle.forward, transform.right), Vector3.Dot(muzzle.forward, transform.up)) * crosshairDeviance;
            ((RectTransform)crosshair.transform).anchoredPosition = position;
        }

        private void Reload()
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer > stats.reloadTime)
            {
                currentMagazine = stats.magazineSize;
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

            var damage = new DamageArgs((int)stats.damage, stats.knockback);

            args.damage = damage;
            args.speed = stats.bulletSpeed;
            args.sprayAngle = stats.spray;
            args.count = stats.bulletCount;
            args.bounces = stats.bounces;
            args.homing = stats.homing;
            args.lifetime = stats.projectileLifetime;

            return args;
        }

        private void SpawnProjectile()
        {
            spawnProjectileEvent?.Invoke();

            lastShootTime = Time.time;

            var view = projectileSpawnPoint ? projectileSpawnPoint : muzzle;
            var runner = NetworkRunner.Instances.Count > 0 ? NetworkRunner.Instances[0] : null;
            var tick = runner ? runner.Tick.Raw : 0;
            var instances = Projectile.Spawn(projectile, owner, view.position + view.forward * 0.1f, muzzle.forward, GetProjectileSpawnArgs(), currentMagazine + tick);
            projectiles.AddRange(instances);

            currentMagazine--;
            reloadTimer = 0f;

            var shuffle = new Shuffler(currentMagazine);

            var recoilData = this.recoilData;
            recoilData.position += new Vector3
            {
                x = translationRecoil.x * Mathf.Sign(shuffle.Next01() - 0.5f),
                y = translationRecoil.y,
                z = translationRecoil.z,
            } * stats.recoil;
            recoilData.rotation += new Vector3
            {
                x = rotationRecoil.x,
                y = rotationRecoil.y * Mathf.Sign(shuffle.Next01() - 0.5f),
                z = rotationRecoil.z * Mathf.Sign(shuffle.Next01() - 0.5f),
            } * stats.recoil;
            this.recoilData = recoilData;

            if (flash) flash.Play();
        }

        private void PlayShootSound()
        {
            if (shootEventBufferIndex >= shootEventBuffer.Length) shootEventBufferIndex = 0;
            var shootEvent = shootEventBuffer[shootEventBufferIndex++];

            if (currentMagazine > 0 || !hasDryFired)
            {
                shootEvent.set3DAttributes(muzzle.To3DAttributes());
                shootEvent.setParameterByName("Magazine", currentMagazine);
                shootEvent.start();
            }

            hasDryFired = currentMagazine == 0;
        }

        public struct RecoilData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 rotation;
            public Vector3 angularVelocity;
        }

        public class ModelData
        {
            public readonly GameObject gameObject;
            public readonly Transform transform;
            public readonly Renderer[] renderers;

            public readonly Transform root;
            public readonly Transform muzzle;
            public readonly Transform leftHandTarget;
            public readonly Transform rightHandTarget;
            public int layer;

            public void ShouldRender(bool state)
            {
                foreach (var r in renderers) r.enabled = state;
            }

            public ModelData(GameObject gameObject, int layer)
            {
                this.gameObject = gameObject;
                this.layer = layer;
                
                transform = gameObject.transform;
                renderers = gameObject.GetComponentsInChildren<Renderer>();

                root = transform.Search("root");
                
                leftHandTarget = transform.Search("Hand.L");
                rightHandTarget = transform.Search("Hand.R");
                muzzle = transform.Search("Muzzle");
                
                foreach (var child in gameObject.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = layer;
                }
            }
        }
    }
}