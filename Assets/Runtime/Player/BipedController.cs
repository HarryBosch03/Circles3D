using Circles3D.Runtime.Networking;
using Circles3D.Runtime.Stats;
using FMOD.Studio;
using FMODUnity;
using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Player
{
    [SelectionBase]
    public class BipedController : NetworkBehaviour
    {
        [Space]
        public float mouseSensitivity = 0.3f;

        [Space]
        public float walkSpeedFactor = 5f / 8f;
        public float jumpHeight = 1.5f;
        public float gravityScale = 2f;
        [Range(0f, 1f)]
        public float airMovementPenalty = 0.75f;
        
        [Space]
        public float characterHeight = 1.8f;
        public float cameraHeight = 1.7f;
        public float characterRadius = 0.2f;
        public float stepHeight = 0.2f;
        public float maxWalkableSlope = 50f;

        [Space]
        public EventReference footstepRef;
        public EventReference landRef;

        private EventInstance footstepSound;
        private EventInstance landSound;
        private bool wasOnGround;
        private Vector3 force;
        
        private StatBoard statboard;
        private RaycastHit groundHit;
        
        private Vector3 interpolationPosition0;
        private Vector3 interpolationPosition1;
        
        [Networked] public Vector3 position { get; set; }
        [Networked] public Vector3 velocity { get; set; }
        [Networked] public bool onGround { get; private set; }
        [Networked] public NetInput input { get; set; }
        [Networked] public NetworkButtons prevButtons { get; set; }
        [Networked] public float fieldOfView { get; set; }
        [Networked] public Vector2 orientation { get; set; }
        public StatBoard.Stats stats => statboard.evaluated;
        public float cameraDutch { get; set; }
        public Transform view { get; private set; }
        public Vector3 center => position + Vector3.up * characterHeight * 0.5f;
        public Vector3 gravity => Physics.gravity * gravityScale * stats.gravity;

        private void Awake()
        {
            view = transform.Find("View");

            statboard = GetBehaviour<StatBoard>();
            
            footstepSound = RuntimeManager.CreateInstance(footstepRef);
            landSound = RuntimeManager.CreateInstance(landRef);
        }

        private void OnDestroy()
        {
            footstepSound.release();
            landSound.release();
        }
        
        private void OnEnable()
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            footstepSound.start();
        }

        private void OnDisable()
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            footstepSound.stop(STOP_MODE.IMMEDIATE);
        }

        public override void FixedUpdateNetwork()
        {
            force = gravity;
            if (GetInput(out NetInput newInput)) input = newInput;
            
            orientation += ComputeOrientationDelta(input.mouseDelta);
            orientation = new Vector2
            {
                x = Mathf.Clamp(orientation.x, -89f, 89f),
                y = orientation.y % 360f,
            };

            CheckForGround();
            Move();
            Jump();
            
            position += velocity * Runner.DeltaTime;
            velocity += force * Runner.DeltaTime;
            
            Collide();
            
            prevButtons = input.buttons;
        }

        private void Collide()
        {
            var collider = new GameObject().AddComponent<CapsuleCollider>();
            collider.transform.SetParent(transform);
            collider.transform.position = position;
            collider.transform.rotation = Quaternion.identity;
            
            collider.center = Vector3.up * (characterHeight + stepHeight) * 0.5f;
            collider.height = characterHeight - stepHeight;
            collider.radius = characterRadius;

            var bounds = collider.bounds;
            var broad = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, ~0);
            foreach (var other in broad)
            {
                if (other.transform.IsChildOf(transform)) continue;
                
                if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, other, other.transform.position, other.transform.rotation, out var normal, out var depth))
                {
                    position += normal * depth;
                    velocity += normal * Mathf.Max(0f, Vector3.Dot(normal, -velocity));
                }
            }
            
            DestroyImmediate(collider.gameObject);
        }

        private void FixedUpdate()
        {
            transform.position = position;
            
            interpolationPosition1 = interpolationPosition0;
            interpolationPosition0 = position;
        }

        private void Update()
        {
            if (onGround && !wasOnGround)
            {
                landSound.set3DAttributes(gameObject.To3DAttributes());
                landSound.start();
            }

            wasOnGround = onGround;
            
            footstepSound.set3DAttributes(gameObject.To3DAttributes());
            footstepSound.setParameterByName("MoveSpeed", onGround ? new Vector2(velocity.x, velocity.z).magnitude / 8f : 0f);
        }

        private Vector2 ComputeOrientationDelta(Vector2 mouseDelta)
        {
            var tangent = Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f);
            return mouseDelta * tangent * mouseSensitivity;
        }
        
        private void Jump()
        {
            var jump = input.buttons.WasPressed(prevButtons, NetInput.Jump);
            if (jump && onGround)
            {
                velocity += Vector3.up * (Mathf.Sqrt(2f * jumpHeight * -gravity.y) - velocity.y);
            }
        }
        
        private void CheckForGround()
        {
            var skinWidth = onGround ? 0.35f : 0f;
            var distance = cameraHeight * 0.5f;

            onGround = false;

            var ray = new Ray(position + Vector3.up * distance, Vector3.down);
            if (velocity.y < 1f && Physics.Raycast(ray, out groundHit, distance + skinWidth, 0b1))
            {
                var dot = Vector3.Dot(groundHit.normal, Vector3.up);
                position = new Vector3(position.x, groundHit.point.y, position.z);

                if (Mathf.Acos(dot) < maxWalkableSlope * Mathf.Deg2Rad)
                {
                    onGround = true;
                    velocity += Vector3.up * Mathf.Max(0f, Vector3.Dot(Vector3.up, -velocity));
                }
                else
                {
                    velocity += groundHit.normal * Mathf.Max(0f, Vector3.Dot(groundHit.normal, -velocity));
                }
            }
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(0f, orientation.y, 0f);

            view.position = Vector3.Lerp(interpolationPosition1, interpolationPosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(orientation) * Quaternion.Euler(0f, 0f, cameraDutch);
        }

        private void Move()
        {
            var moveInput = Vector2.ClampMagnitude(input.movement, 1f);

            var acceleration = stats.acceleration;
            if (!onGround) acceleration *= 1f - airMovementPenalty;
            
            var orientation = Quaternion.Euler(0f, this.orientation.y, 0f);

            var moveSpeed = stats.moveSpeed;
            if (!input.buttons.IsSet(NetInput.Run)) moveSpeed *= walkSpeedFactor;
            
            var target = orientation * new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            var force = (target - velocity) * acceleration;
            force.y = 0f;

            if (!onGround) force *= moveInput.magnitude;

            this.force += force;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(true);

            this.position = position;
            velocity = Vector3.zero;
            orientation = rotation.eulerAngles;
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            enabled = true;
            this.position = position;
            
            velocity = Vector3.zero;
            orientation = rotation.eulerAngles;
        }
    }
}