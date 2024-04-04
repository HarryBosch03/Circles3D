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
        [Space]
        public PlayerAvatar avatar;
        public Canvas deathCanvas;
        public CanvasGroup deathUI;
        public Button respawnButton;
        public TMP_Text deathReasonText;

        public string displayName => name;

        public static event Action<PlayerInstance, IDamageable.DamageReport> PlayerDealtDamageEvent;

        private void Awake()
        {
            avatar = GetComponentInChildren<PlayerAvatar>();
        }

        private void Start() { respawnButton.onClick.AddListener(RespawnLocal); }

        private void RespawnLocal() { RpcRespawnServer(); }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RpcRespawnServer()
        {
            if (!Gamemode.current.CanRespawn(this)) return;

            var respawnPosition = Gamemode.current.GetSpawnpoint(this);
            RpcRespawnAll(respawnPosition.position, respawnPosition.rotation);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcRespawnAll(Vector3 position, Quaternion rotation)
        {
            SetDeathReason(null);
            avatar.owningPlayerInstance = this;
            avatar.gameObject.SetActive(true);
            avatar.transform.position = position;
            avatar.transform.rotation = rotation;
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

        private void OnProjectileDealtDamage(Projectile projectile, RaycastHit hit, IDamageable.DamageReport report)
        {
            if (projectile.shooter != avatar) return;
            PlayerDealtDamageEvent?.Invoke(this, report);
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                var cursorLockMode = CursorLockMode.None;
                if (avatar.gameObject.activeSelf)
                {
                    cursorLockMode = CursorLockMode.Locked;
                    deathCanvas.gameObject.SetActive(false);
                    deathUI.alpha = 0f;
                }
                else
                {
                    deathCanvas.gameObject.SetActive(true);
                    deathUI.alpha = Mathf.MoveTowards(deathUI.alpha, 1f, Time.deltaTime * 10f);
                    if (Keyboard.current.spaceKey.wasPressedThisFrame) RespawnLocal();
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