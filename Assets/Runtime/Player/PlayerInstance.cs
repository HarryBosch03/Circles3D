using System;
using Runtime.Damage;
using Runtime.Gamemodes;
using Runtime.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Runtime.Player
{
    public class PlayerInstance : MonoBehaviour
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

        public bool isOwner => true;

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

        private void RequestRespawn() { Gamemode.current.RespawnPlayer(this); }

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

        private void Update()
        {
            if (isOwner)
            {
                var cursorLockMode = CursorLockMode.None;
                if (avatar.gameObject.activeSelf)
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

                if (Cursor.lockState != cursorLockMode) Cursor.lockState = cursorLockMode;
            }
            else
            {
                deathCanvas.gameObject.SetActive(false);
            }
        }

        private void SetDeathReason(string text) => deathReasonText.text = (text ?? "Killed by Dying").ToUpper();
    }
}