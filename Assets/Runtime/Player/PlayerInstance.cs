using System;
using Fusion;
using Runtime.Damage;
using Runtime.Gamemodes;
using Runtime.Networking;
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

        private Camera mainCam;
        public string displayName => name;

        public static event Action<PlayerInstance, IDamageable.DamageReport> PlayerDealtDamageEvent;

        [Networked]
        public NetworkState netState { get; set; }
        public bool isOwner => HasInputAuthority;

        private void Awake()
        {
            mainCam = Camera.main;
            avatar = GetComponentInChildren<PlayerAvatar>();
        }

        private void Start()
        {
            respawnButton.onClick.AddListener(RpcRequestRespawn);
            avatar.gameObject.SetActive(false);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RpcRequestRespawn() { Gamemode.current.RespawnPlayer(this); }

        public void SpawnAt(Vector3 position, Quaternion rotation)
        {
            SetDeathReason(null);

            avatar.gameObject.SetActive(true);
            avatar.transform.position = position;
            avatar.transform.rotation = rotation;
            avatar.owningPlayerInstance = this;
        }

        private void OnEnable()
        {
            Projectile.projectileDealtDamageEvent += OnProjectileDealtDamage;
            HealthController.DiedEvent += OnDied;

            deathCanvas.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Projectile.projectileDealtDamageEvent -= OnProjectileDealtDamage;
            HealthController.DiedEvent -= OnDied;
        }

        private void OnDied(HealthController victim, GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (!victim) return;
            if (victim.gameObject != avatar.gameObject) return;

            SetDeathReason(((PlayerHealthController)victim).reasonForDeath);
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

            PlayerDealtDamageObserverRPC(report);
        }

        private void PlayerDealtDamageObserverRPC(IDamageable.DamageReport report) { PlayerDealtDamageEvent?.Invoke(this, report); }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                netState = new NetworkState
                {
                    alive = avatar.gameObject.activeSelf,
                };
            }
            else
            {
                avatar.gameObject.SetActive(netState.alive);
            }

            if (!avatar.gameObject.activeSelf && HasInputAuthority)
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
            if (!avatar.gameObject.activeSelf && HasInputAuthority)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame) RpcRequestRespawn();
            }
        }

        private void SetDeathReason(string text) => deathReasonText.text = (text ?? "Killed by Dying").ToUpper();

        public struct NetworkState : INetworkStruct
        {
            public bool alive;
        }
    }
}