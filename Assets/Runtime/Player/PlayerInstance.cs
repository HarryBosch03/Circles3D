using System;
using Fusion;
using Runtime.Damage;
using Runtime.Gamemodes;
using Runtime.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Runtime.Player
{
    public class PlayerInstance : NetworkBehaviour
    {
        public float mouseSensitivity = 0.3f;

        [Space]
        public PlayerAvatar avatar;
        public Canvas deathCanvas;
        public CanvasGroup deathUI;
        public Button respawnButton;
        public TMP_Text deathReasonText;

        private bool respawn;
        
        private Camera mainCam;
        public string displayName => name;

        public static event Action<PlayerInstance, IDamageable.DamageReport> PlayerDealtDamageEvent;

        public bool isOwner => HasInputAuthority;

        private void Awake()
        {
            mainCam = Camera.main;
            avatar = GetComponentInChildren<PlayerAvatar>();
        }

        private void Start()
        {
            respawnButton.onClick.AddListener(RpcRequestRespawn);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RpcRequestRespawn()
        {
            respawn = true;
            Gamemode.current.RespawnPlayer(this);
        }

        public void SpawnAt(Vector3 position, Quaternion rotation)
        {
            SetDeathReason(null);
            avatar.owningPlayerInstance = this;
            avatar.Spawn(position, rotation);
        }

        private void OnEnable()
        {
            deathCanvas.gameObject.SetActive(false);
        }

        public override void Spawned()
        {
            Projectile.ProjectileDealtDamageEvent += OnProjectileDealtDamage;
            avatar.health.DiedEvent += OnDied;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Projectile.ProjectileDealtDamageEvent -= OnProjectileDealtDamage;
            avatar.health.DiedEvent -= OnDied;
        }

        private void OnDied(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            SetDeathReason(avatar.health.reasonForDeath);
        }

        private void OnPlayerKilledByPlayer(PlayerInstance killer, PlayerInstance victim, IDamageable.DamageReport report)
        {
            if (victim == this)
            {
                SetDeathReason($"Killed by {killer.displayName}");
            }
        }

        private void OnProjectileDealtDamage(Projectile projectile, RaycastHit hit, IDamageable.DamageReport report)
        {
            if (projectile.shooter != avatar) return;

            PlayerDealtDamageEvent?.Invoke(this, report);
        }

        public override void FixedUpdateNetwork()
        {
            avatar.owningPlayerInstance = this;

            if (respawn)
            {
                Gamemode.current.RespawnPlayer(this);
                respawn = false;
            }
            
            if (!avatar.health.alive && HasInputAuthority)
            {
                deathCanvas.gameObject.SetActive(true);
                deathUI.alpha = Mathf.MoveTowards(deathUI.alpha, 1f, Time.deltaTime * 10f);
            }
            else
            {
                deathCanvas.gameObject.SetActive(false);
                deathUI.alpha = 0f;
            }
        }

        private void Update()
        {
            if (!avatar.health.alive && HasInputAuthority)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame) RpcRequestRespawn();
            }
        }

        private void SetDeathReason(string text) => deathReasonText.text = (text ?? "Killed by Dying").ToUpper();
    }
}