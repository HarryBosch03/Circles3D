using System;
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
        public PlayerAvatar avatar;
        public Canvas deathCanvas;
        public CanvasGroup deathUI;
        public Button respawnButton;
        public TMP_Text deathReasonText;

        private Camera mainCam;
        private string displayName => name;

        public static event Action<PlayerInstance, IDamageable.DamageReport> PlayerDealtDamageEvent;
        public static event Action<PlayerInstance, PlayerInstance, IDamageable.DamageReport> PlayerKilledByPlayerEvent;
        
        private void Awake()
        {
            mainCam = Camera.main;
            avatar = GetComponentInChildren<PlayerAvatar>();
        }

        private void Start()
        {
            respawnButton.onClick.AddListener(RequestRespawn); 
            avatar.gameObject.SetActive(false);
        }

        private void RequestRespawn()
        {
            Gamemode.current.RespawnPlayer(this);
        }
        
        [ObserversRpc(ExcludeServer = false, ExcludeOwner = false, RunLocally = true)]
        public void Respawn(Vector3 position, Quaternion rotation)
        {
            SetDeathReason(null);
            avatar.Respawn(position, rotation);
        }

        private void OnEnable()
        {
            Projectile.projectileDealtDamageEvent += OnProjectileDealtDamage;
            PlayerKilledByPlayerEvent += OnPlayerKilledByPlayer;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None; 
            Projectile.projectileDealtDamageEvent -= OnProjectileDealtDamage;
            PlayerKilledByPlayerEvent -= OnPlayerKilledByPlayer;
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
            if (projectile.shooter != avatar) return;

            PlayerDealtDamageObserverRPC(report);
        }
        
        [ObserversRpc(RunLocally = true)]
        private void PlayerDealtDamageObserverRPC(IDamageable.DamageReport report)
        {
            PlayerDealtDamageEvent?.Invoke(this, report);
            
            var otherAvatar = report.victim ? report.victim.GetComponent<PlayerAvatar>() : null;
            var otherPlayer = otherAvatar ? otherAvatar.owningPlayerInstance : null;
            if (!otherPlayer) return;
            
            PlayerKilledByPlayerEvent?.Invoke(this, otherPlayer, report);
        }

        private void Update()
        {
            if (IsOwner)
            {
                var cursorLockMode = CursorLockMode.None;
                if (avatar)
                {
                    if (avatar.isAlive)
                    {
                        cursorLockMode = CursorLockMode.Locked;
                        avatar.owningPlayerInstance = this;

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

                        avatar.input = input;

                        deathCanvas.gameObject.SetActive(false);
                        deathUI.alpha = 0f;
                    }
                    else
                    {
                        deathCanvas.gameObject.SetActive(true);
                        deathUI.alpha = Mathf.MoveTowards(deathUI.alpha, 1f, Time.deltaTime * 10f);
                        if (Keyboard.current.spaceKey.wasPressedThisFrame) RequestRespawn();
                    }
                }

                if (Cursor.lockState != cursorLockMode) Cursor.lockState = cursorLockMode;
            }
            else
            {
                deathCanvas.gameObject.SetActive(false);
            }
        }

        private void SetDeathReason(string text)
        {
            deathReasonText.text = text ?? "Killed by Death";
        }
    }
}