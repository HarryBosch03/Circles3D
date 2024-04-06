using Fusion;
using Runtime.Networking;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Player
{
    [SelectionBase]
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
        public bool isAlive => gameObject.activeSelf;

        [Networked]
        public NetworkData netData { get; set; }
        
        [Networked]
        public NetInput input { get; set; }
        public Transform view { get; private set; }
        public BipedController movement { get; private set; }
        public Gun gun { get; private set; }
        public bool onGround { get; private set; }
        public PlayerInstance owningPlayerInstance { get; set; }

        private void Awake()
        {
            camera = Camera.main;
            gun = GetComponentInChildren<Gun>();
            movement = GetComponent<BipedController>();

            view = transform.Find("View");
        }

        private void OnEnable()
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnDisable()
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetInput newInput)) input = newInput;
            
            var netData = this.netData;
            
            if (gun)
            {
                gun.SetFirstPerson(HasInputAuthority);

                if (input.buttons.IsSet(InputButton.Shoot)) gun.Shoot();
                gun.aiming = input.buttons.IsSet(InputButton.Aim);
                gun.projectileSpawnPoint = view;

                var recoil = gun.recoilData.angularVelocity;
                movement.OffsetRotation(new Vector2(-recoil.y, recoil.x) * feltRecoil * Runner.DeltaTime);
            }

            this.netData = netData;
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                camera.fieldOfView = CalculateFieldOfView();
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

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            movement.Teleport(position, rotation);
        }

        public struct NetworkData : INetworkStruct
        {
            
        }
    }
}