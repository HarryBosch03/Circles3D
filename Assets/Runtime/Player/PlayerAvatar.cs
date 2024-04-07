using System;
using Fusion;
using Runtime.Damage;
using Runtime.Networking;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Player
{
    [SelectionBase]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BipedController))]
    public class PlayerAvatar : NetworkBehaviour
    {
        [Space]
        public float feltRecoil = 1.0f;

        [Space]
        public float baseFieldOfView = 90f;
        public float aimFieldOfView = 60f;

        private new Camera camera;
        private RaycastHit groundHit;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;
        
        private Transform model;

        [Networked]
        public NetInput input { get; set; }
        public BipedController movement { get; private set; }
        public PlayerHealthController health { get; private set; }
        public Transform view { get; private set; }
        public Gun gun { get; private set; }
        public PlayerInstance owningPlayerInstance { get; set; }

        private void Awake()
        {
            camera = Camera.main;
            gun = GetComponentInChildren<Gun>();
            movement = GetComponent<BipedController>();
            health = GetComponent<PlayerHealthController>();

            model = transform.Find("Model");

            view = transform.Find("View");
        }
        
        private void OnEnable()
        {
            health.DiedEvent += OnDied;
        }

        private void OnDisable()
        {
            health.DiedEvent -= OnDied;
        }

        public override void Spawned()
        {
            movement.enabled = false;
            gun.SetVisible(false);
            SetModelVisibility(false);
        }

        private void SetModelVisibility(bool enabled)
        {
            model.gameObject.SetActive(enabled);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetInput newInput)) input = newInput;

            if (health.alive)
            {
                movement.enabled = true;
                movement.fieldOfView = CalculateFieldOfView();
                
                gun.SetVisible(true, HasInputAuthority);
                SetModelVisibility(true);
                
                if (gun)
                {
                    gun.SetVisible(true, HasInputAuthority);

                    if (input.buttons.IsSet(InputButton.Shoot)) gun.Shoot();
                    gun.aiming = input.buttons.IsSet(InputButton.Aim);
                    gun.projectileSpawnPoint = view;

                    var recoil = gun.recoilData.angularVelocity;
                    movement.OffsetRotation(new Vector2(-recoil.x, recoil.y) * feltRecoil * Runner.DeltaTime);
                }
            }
            else
            {
                movement.enabled = false;
                gun.SetVisible(false);
                SetModelVisibility(false);
            }
        }

        private void FixedUpdate()
        {
            if (!HasInputAuthority && !HasStateAuthority)
            {
                movement.enabled = health.alive;
                gun.SetVisible(health.alive, false);
                SetModelVisibility(health.alive);
            }
        }

        private void LateUpdate()
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = health.alive ? CursorLockMode.Locked : CursorLockMode.None;
                camera.transform.position = movement.view.position;
                camera.transform.rotation = movement.view.rotation;
                camera.fieldOfView = movement.fieldOfView;
            }
        }

        private float CalculateFieldOfView()
        {
            var aim = gun ? gun.aimPercent : 0f;
            var fieldOfView = Mathf.Lerp(baseFieldOfView, aimFieldOfView, aim);

            var zoom = gun ? gun.zoom : 1f;
            var tangent = Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f);
            fieldOfView = Mathf.Atan(tangent / zoom) * Mathf.Rad2Deg * 2f;

            return fieldOfView;
        }

        public void Respawn(Vector3 position, Quaternion rotation) { movement.Teleport(position, rotation); }

        private void OnDied(GameObject invoker, DamageArgs args, Vector3 point, Vector3 velocity)
        {
            gun.SetVisible(false);
            SetModelVisibility(false);
        }
        
        public void Spawn(Vector3 position, Quaternion rotation)
        {
            movement.Spawn(position, rotation);
            
            health.Spawn();
        }
    }
}