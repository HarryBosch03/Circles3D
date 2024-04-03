using System;
using FishNet;
using FishNet.Object;
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
        public PlayerAvatar avatarPrefab;
        public PlayerAvatar currentAvatar;
        public Canvas deathCanvas;
        public CanvasGroup deathUI;
        public Button respawnButton;
        public TMP_Text deathReasonText;

        private Camera mainCam;
        public string displayName => name;

        public static event Action<PlayerInstance, IDamageable.DamageReport> PlayerDealtDamageEvent;

        private void Awake() { mainCam = Camera.main; }

        private void Start() { respawnButton.onClick.AddListener(RequestRespawn); }

        private void RequestRespawn() { Gamemode.current.RespawnPlayer(this); }

        [ObserversRpc(ExcludeServer = false, ExcludeOwner = false, RunLocally = true)]
        public void Respawn(Vector3 position, Quaternion rotation)
        {
            SetDeathReason(null);
            if (IsServer)
            {
                currentAvatar = Instantiate(avatarPrefab, position, rotation);
                InstanceFinder.ServerManager.Spawn(currentAvatar.NetworkObject, Owner);
                SetAvatar(currentAvatar.NetworkObject);
                currentAvatar.owningPlayerInstance = this;
            }
        }

        [ObserversRpc(RunLocally = false, ExcludeServer = true)]
        private void SetAvatar(NetworkObject avatarNetObject)
        {
            currentAvatar = avatarNetObject.GetComponent<PlayerAvatar>();
            currentAvatar.owningPlayerInstance = this;
        }

        private void OnEnable()
        {
            Projectile.projectileDealtDamageEvent += OnProjectileDealtDamage;
            HealthController.DiedEvent += OnDied;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Projectile.projectileDealtDamageEvent -= OnProjectileDealtDamage;
            HealthController.DiedEvent -= OnDied;
        }

        private void OnDied(HealthController victim, NetworkObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            if (!victim) return;
            if (!currentAvatar) return;
            if (victim.gameObject != currentAvatar.gameObject) return;
            
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
            if (!IsServer) return;
            if (projectile.shooter != currentAvatar) return;

            PlayerDealtDamageObserverRPC(report);
        }

        [ObserversRpc(RunLocally = true)]
        private void PlayerDealtDamageObserverRPC(IDamageable.DamageReport report)
        {
            PlayerDealtDamageEvent?.Invoke(this, report);
        }

        private void Update()
        {
            if (IsOwner)
            {
                var cursorLockMode = CursorLockMode.None;
                if (currentAvatar)
                {
                    cursorLockMode = CursorLockMode.Locked;

                    PlayerAvatar.InputData input;
                    var kb = Keyboard.current;
                    var m = Mouse.current;

                    input.movement.x = kb.dKey.ReadValue() - kb.aKey.ReadValue();
                    input.movement.y = kb.wKey.ReadValue() - kb.sKey.ReadValue();

                    var tangent = Mathf.Tan(mainCam.fieldOfView * Mathf.Deg2Rad * 0.5f);
                    input.lookDelta = m.delta.ReadValue() * mouseSensitivity * tangent;

                    input.run = kb.leftShiftKey.isPressed;
                    input.jump = kb.spaceKey.isPressed;

                    input.shoot = m.leftButton.isPressed;
                    input.aim = m.rightButton.isPressed;

                    currentAvatar.input = input;

                    deathCanvas.gameObject.SetActive(false);
                    deathUI.alpha = 0f;
                }
                else
                {
                    deathCanvas.gameObject.SetActive(true);
                    deathUI.alpha = Mathf.MoveTowards(deathUI.alpha, 1f, Time.deltaTime * 10f);
                    if (Keyboard.current.spaceKey.wasPressedThisFrame) RequestRespawn();
                }

                if (Cursor.lockState != cursorLockMode) Cursor.lockState = cursorLockMode;
            }
            else
            {
                deathCanvas.gameObject.SetActive(false);
            }
        }

        [ObserversRpc(RunLocally = true)]
        private void SetDeathReason(string text) => deathReasonText.text = (text ?? "Killed by Dying").ToUpper();
    }
}