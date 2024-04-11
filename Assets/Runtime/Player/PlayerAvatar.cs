using System;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Networking;
using Circles3D.Runtime.Stats;
using Circles3D.Runtime.Weapons;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Circles3D.Runtime.Player
{
    [SelectionBase]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(BipedController))]
    public class PlayerAvatar : NetworkBehaviour
    {
        [Space]
        public float feltRecoil = 1.0f;
        public float ragdollCamSmoothing = 1f;

        [Space]
        public float baseFieldOfView = 90f;
        public float aimFieldOfView = 60f;
        public float baseViewportFieldOfView = 43f;
        public float aimViewportFieldOfView = 10f;

        private Camera mainCam;
        private Camera viewportCam;
        private RaycastHit groundHit;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;

        private Transform model;

        [Networked] public NetInput input { get; set; }
        [Networked] public NetworkButtons previousButtons { get; set; }
        public BipedController movement { get; private set; }
        public PlayerHealthController health { get; private set; }
        public StatBoard statboard { get; set; }
        public Transform view { get; private set; }
        public Gun gun { get; private set; }
        public PlayerInstance owningPlayerInstance { get; set; }
        public bool activeViewer { get; set; }
        public float mass => 80f;

        public static event Action<PlayerAvatar, GameObject, DamageArgs, Vector3, Vector3> DeathEvent;
        
        private void Awake()
        {
            mainCam = Camera.main;
            viewportCam = mainCam.transform.GetChild(0).GetComponent<Camera>();

            gun = GetComponentInChildren<Gun>();
            movement = GetComponent<BipedController>();
            health = GetComponent<PlayerHealthController>();
            statboard = GetComponent<StatBoard>();

            model = transform.Find("Model");

            view = transform.Find("View");
        }

        private void OnEnable() { health.DiedEvent += OnDied; }

        private void OnDisable() { health.DiedEvent -= OnDied; }

        public override void Spawned()
        {
            movement.enabled = false;
            gun.SetVisible(false);
            SetModelVisibility(false);

            Runner.SetIsSimulated(Object, true);
        }

        private void SetModelVisibility(bool enabled) { model.gameObject.SetActive(enabled); }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetInput newInput)) input = newInput;

            if (health.alive)
            {
                movement.enabled = true;
                movement.fieldOfView = CalculateFieldOfView();

                gun.SetVisible(true, activeViewer);
                SetModelVisibility(true);

                if (gun)
                {
                    gun.SetVisible(true, activeViewer);

                    if (input.buttons.IsSet(NetInput.Shoot)) gun.Shoot();
                    gun.aiming = input.buttons.IsSet(NetInput.Aim);
                    gun.projectileSpawnPoint = view;

                    var recoil = gun.recoilData.angularVelocity;
                    movement.orientation += (new Vector2(-recoil.x, recoil.y) * feltRecoil * Runner.DeltaTime);
                }
            }
            else
            {
                movement.enabled = false;
                gun.SetVisible(false);
                SetModelVisibility(false);
            }

            previousButtons = input.buttons;
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (HasInputAuthority)
            {
                var kb = Keyboard.current;
                if (kb.tabKey.isPressed)
                {
                    if (kb.vKey.wasPressedThisFrame) ToggleActiveViewer();
                }
            }
#endif

            if (activeViewer)
            {
                if (health.alive)
                {
                    mainCam.transform.position = movement.view.position;
                    mainCam.transform.rotation = movement.view.rotation;
                    mainCam.fieldOfView = movement.fieldOfView;

                    var viewportFov = Mathf.Lerp(baseViewportFieldOfView, gun.currentSight ? gun.currentSight.viewportFov : aimViewportFieldOfView, gun.aimPercent);
                    var zOffset = Mathf.Lerp(0f, gun.currentSight ? gun.currentSight.zOffset : 0f, gun.aimPercent);

                    viewportCam.fieldOfView = viewportFov;
                    viewportCam.transform.localPosition = Vector3.forward * zOffset;
                }
                else if (health.latestRagdollHead)
                {
                    mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, health.latestRagdollHead.position, Time.deltaTime / ragdollCamSmoothing);
                    mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, health.latestRagdollHead.rotation, Time.deltaTime / ragdollCamSmoothing);
                }
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
            if (HasInputAuthority) InputManager.SetIsControllingPlayer(false);
            gun.SetVisible(false);
            SetModelVisibility(false);

            DeathEvent?.Invoke(this, invoker, args, point, velocity);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, InvokeLocal = true)]
        public void SpawnRpc(Vector3 position, Quaternion rotation)
        {
            if (HasInputAuthority)
            {
                InputManager.SetIsControllingPlayer(true);
                activeViewer = true;
            }

            movement.Spawn(position, rotation);
            health.Spawn();
        }

        [ContextMenu("Toggle Active Viewer")]
        private void ToggleActiveViewer() => activeViewer = !activeViewer;
    }
}