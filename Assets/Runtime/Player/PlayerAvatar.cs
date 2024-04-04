using System;
using Fusion;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerAvatar : NetworkBehaviour
    {
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

        [Networked]
        public NetworkState netState { get; set; }
        
        private new Camera camera;
        private bool jumpFlag;
        private RaycastHit groundHit;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;
        public bool isAlive => gameObject.activeSelf;

        public NetworkInputData input { get; set; }
        public Vector2 orientation { get; set; }

        public Transform view { get; private set; }
        public Gun gun { get; private set; }
        public Rigidbody body { get; private set; }
        public bool onGround { get; private set; }
        private Vector3 gravity => Physics.gravity * gravityScale;
        public PlayerInstance owningPlayerInstance { get; set; }

        private void Awake()
        {
            camera = Camera.main;
            body = GetComponent<Rigidbody>();
            gun = GetComponentInChildren<Gun>();

            view = transform.Find("View");
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                this.input = input;
                body.constraints = RigidbodyConstraints.FreezeRotation;
                
                CheckForGround();
                Move();
                Jump();
                UpdateCamera();

                if (gun)
                {
                    if (input.shoot) gun.Shoot();
                    gun.aiming = input.aim;
                    gun.projectileSpawnPoint = view;

                    var recoil = gun.recoilData.angularVelocity;
                    orientation += new Vector2(-recoil.y, recoil.x) * feltRecoil * Time.deltaTime;
                }

                body.AddForce(gravity - Physics.gravity, ForceMode.Acceleration);

                input.jump = false;
                this.input = input;

                if (HasStateAuthority)
                {
                    netState = new NetworkState
                    {
                        position = body.position,
                        orientation = orientation,
                        velocity = body.velocity,
                    };
                }
                else
                {
                    body.position = netState.position;
                    orientation = netState.orientation;
                    body.velocity = netState.velocity;
                }
            }
        }

        private void FixedUpdate()
        {
            bodyInterpolatePosition1 = bodyInterpolatePosition0;
            bodyInterpolatePosition0 = body.position;
        }

        private void UpdateCamera() { transform.rotation = Quaternion.Euler(0f, orientation.x, 0f); }

        private void Jump()
        {
            if (input.jump && !jumpFlag)
            {
                if (onGround)
                {
                    var force = Vector3.up * Mathf.Sqrt(2f * jumpHeight * -gravity.y);
                    body.AddForce(force, ForceMode.VelocityChange);
                }
            }

            jumpFlag = input.jump;
        }

        private void Update()
        {
            if (HasInputAuthority)
            {
                var tangent = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
                orientation += Mouse.current.delta.ReadValue() * tangent * 0.3f;
            }

            orientation = new Vector2
            {
                x = orientation.x % 360f,
                y = Mathf.Clamp(orientation.y, -90f, 90f),
            };

            view.position = Vector3.Lerp(bodyInterpolatePosition1, bodyInterpolatePosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(-orientation.y, orientation.x, 0f);

            if (HasInputAuthority)
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

        private void CheckForGround()
        {
            var skinWidth = onGround ? 0.35f : 0f;
            var distance = cameraHeight * 0.5f;

            onGround = false;

            var ray = new Ray(transform.position + Vector3.up * distance, Vector3.down);
            if (body.velocity.y < 1f && Physics.Raycast(ray, out groundHit, distance + skinWidth))
            {
                var dot = Vector3.Dot(groundHit.normal, Vector3.up);
                body.position = new Vector3(body.position.x, groundHit.point.y, body.position.z);

                if (Mathf.Acos(dot) < maxWalkableSlope * Mathf.Deg2Rad)
                {
                    onGround = true;
                    body.velocity += Vector3.up * Mathf.Max(0f, Vector3.Dot(Vector3.up, -body.velocity));
                    if (groundHit.rigidbody)
                    {
                        groundHit.rigidbody.AddForceAtPosition(gravity * body.mass, groundHit.point);
                    }
                }
                else
                {
                    body.velocity += groundHit.normal * Mathf.Max(0f, Vector3.Dot(groundHit.normal, -body.velocity));
                }
            }
        }

        private void Move()
        {
            var moveInput = Vector2.ClampMagnitude(input.movement, 1f);

            var acceleration = 2f / moveAcceleration;
            if (!onGround) acceleration *= 1f - airMovementPenalty;

            var moveSpeed = input.run ? runSpeed : walkSpeed;
            var target = transform.TransformVector(moveInput.x, 0f, moveInput.y) * moveSpeed;
            var force = (target - body.velocity) * acceleration;
            force.y = 0f;

            if (!onGround) force *= moveInput.magnitude;

            body.AddForce(force, ForceMode.Acceleration);
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = rotation;
            body.velocity = Vector3.zero;
        }

        public struct NetworkState : INetworkStruct
        {
            public Vector3 position;
            public Vector2 orientation;
            public Vector3 velocity;
        }
    }
}