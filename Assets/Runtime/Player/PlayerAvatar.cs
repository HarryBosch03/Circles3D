using System;
using Fusion;
using Runtime.Networking;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Player
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerAvatar : NetworkBehaviour
    {
        [Space]
        public float mouseSensitivity = 0.3f;
        
        [Space]
        public float walkSpeed = 6f;
        public float runSpeed = 10f;
        public float moveAcceleration = 0.1f;
        public float jumpHeight = 2.5f;
        public float gravityScale = 2.5f;
        [Range(0f, 1f)]
        public float airMovementPenalty;

        [Space]
        public float cameraHeight = 1.8f;
        public float maxWalkableSlope = 40f;

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
        public Vector2 orientation => netData.orientation;

        public Transform view { get; private set; }
        public Gun gun { get; private set; }
        public Rigidbody body { get; private set; }
        public bool onGround { get; private set; }
        private Vector3 gravity => Physics.gravity * gravityScale;
        public PlayerInstance owningPlayerInstance { get; set; }

        public bool isOwner => owningPlayerInstance && owningPlayerInstance.isOwner;

        private void Awake()
        {
            camera = Camera.main;
            body = GetComponent<Rigidbody>();
            gun = GetComponentInChildren<Gun>();

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
            
            body.constraints = RigidbodyConstraints.FreezeRotation;
            var tangent = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            netData.orientation += input.orientationDelta * tangent * mouseSensitivity;

            Move(ref netData);
            Jump(ref netData);
            UpdateCamera();

            if (gun)
            {
                gun.SetFirstPerson(isOwner);

                if (input.buttons.IsSet(InputButton.Shoot)) gun.Shoot();
                gun.aiming = input.buttons.IsSet(InputButton.Aim);
                gun.projectileSpawnPoint = view;

                var recoil = gun.recoilData.angularVelocity;
                netData.orientation += new Vector2(-recoil.y, recoil.x) * feltRecoil * Runner.DeltaTime;
            }

            netData.orientation = new Vector2
            {
                x = netData.orientation.x % 360f,
                y = Mathf.Clamp(netData.orientation.y, -90f, 90f),
            };
            
            netData.velocity += gravity * Runner.DeltaTime;

            CheckForGround(ref netData);
            this.netData = netData;
            
            netData.Apply(this);
        }

        private void FixedUpdate()
        {
            if (HasStateAuthority)
            {
                var netData = this.netData;
                netData.position = body.position;
                netData.velocity = body.velocity;
                this.netData = netData;
            }
            
            bodyInterpolatePosition1 = bodyInterpolatePosition0;
            bodyInterpolatePosition0 = body.position;
        }

        private void UpdateCamera() { body.rotation = Quaternion.Euler(0f, orientation.x, 0f); }

        private void Jump(ref NetworkData netData)
        {
            var jump = input.buttons.IsSet(InputButton.Jump);
            if (jump && !netData.jumpFlag)
            {
                if (!onGround) return;

                var force = Vector3.up * Mathf.Sqrt(2f * jumpHeight * -gravity.y);
                netData.velocity += force;
            }

            netData.jumpFlag = jump;
        }

        private void Update()
        {
            view.position = Vector3.Lerp(bodyInterpolatePosition1, bodyInterpolatePosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(-orientation.y, orientation.x, 0f);

            if (isOwner)
            {
                camera.transform.position = view.position;
                camera.transform.rotation = view.rotation;
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

        private void CheckForGround(ref NetworkData netData)
        {
            var skinWidth = onGround ? 0.35f : 0f;
            var distance = cameraHeight * 0.5f;

            onGround = false;

            var ray = new Ray(transform.position + Vector3.up * distance, Vector3.down);
            if (netData.velocity.y < 1f && Physics.Raycast(ray, out groundHit, distance + skinWidth))
            {
                var dot = Vector3.Dot(groundHit.normal, Vector3.up);
                netData.position = new Vector3(netData.position.x, groundHit.point.y, netData.position.z);

                if (Mathf.Acos(dot) < maxWalkableSlope * Mathf.Deg2Rad)
                {
                    onGround = true;
                    netData.velocity += Vector3.up * Mathf.Max(0f, Vector3.Dot(Vector3.up, -netData.velocity));
                    if (groundHit.rigidbody)
                    {
                        groundHit.rigidbody.AddForceAtPosition(gravity * body.mass, groundHit.point);
                    }
                }
                else
                {
                    netData.velocity += groundHit.normal * Mathf.Max(0f, Vector3.Dot(groundHit.normal, -netData.velocity));
                }
            }
        }

        private void Move(ref NetworkData netData)
        {
            var moveInput = Vector2.ClampMagnitude(input.movement, 1f);

            var acceleration = 2f / moveAcceleration;
            if (!onGround) acceleration *= 1f - airMovementPenalty;

            var orientation = Quaternion.Euler(0f, netData.orientation.x, 0f);
            
            var moveSpeed = input.buttons.IsSet(InputButton.Run) ? runSpeed : walkSpeed;
            var target = orientation * new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            var force = (target - netData.velocity) * acceleration;
            force.y = 0f;

            if (!onGround) force *= moveInput.magnitude;

            netData.velocity += force * Runner.DeltaTime;
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = rotation;
            body.velocity = Vector3.zero;
        }

        public struct NetworkData : INetworkStruct
        {
            public Vector3 position;
            public Vector3 velocity;
            public bool jumpFlag;
            
            public Vector2 orientation;

            public void Apply(PlayerAvatar avatar)
            {
                avatar.body.position = position;
                avatar.body.velocity = velocity;
            }
        }
    }
}