using Fusion;
using Runtime.Networking;
using UnityEngine;

namespace Runtime.Player
{
    [SelectionBase]
    public class BipedController : NetworkBehaviour
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
        public float characterHeight = 1.8f;
        public float cameraHeight = 1.7f;
        public float characterRadius = 0.2f;
        public float stepHeight = 0.2f;
        public float maxWalkableSlope = 40f;

        private new Camera camera;
        private RaycastHit groundHit;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;
        
        [Networked]
        public NetworkData netData { get; set; }
        
        [Networked]
        public NetInput input { get; set; }
        public Transform view { get; private set; }
        public bool onGround { get; private set; }
        public Vector2 orientation => netData.orientation;
        public Vector3 center => netData.position + Vector3.up * characterHeight * 0.5f;
        private Vector3 gravity => Physics.gravity * gravityScale;
        
        private void Awake()
        {
            camera = Camera.main;
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
            
            var tangent = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            netData.orientation += input.orientationDelta * tangent * mouseSensitivity;

            CheckForGround(ref netData);
            Move(ref netData);
            Jump(ref netData);
            Integrate(ref netData);
            Collide(ref netData);

            netData.orientation = new Vector2
            {
                x = netData.orientation.x % 360f,
                y = Mathf.Clamp(netData.orientation.y, -90f, 90f),
            };
            
            this.netData = netData;
        }

        private void FixedUpdate()
        {
            transform.position = netData.position;
            
            bodyInterpolatePosition1 = bodyInterpolatePosition0;
            bodyInterpolatePosition0 = netData.position;
        }
        
        private void Integrate(ref NetworkData netData)
        {
            netData.position += netData.velocity * Runner.DeltaTime;
            netData.velocity += netData.force * Runner.DeltaTime;
            netData.force = gravity;
        }

        private void Collide(ref NetworkData netData)
        {
            var collider = gameObject.AddComponent<CapsuleCollider>();
            collider.center = Vector3.up * (characterHeight + stepHeight) * 0.5f;
            collider.height = characterHeight - stepHeight;
            collider.radius = characterRadius;

            for (var i = 0; i < 6; i++)
            {
                var bounds = collider.bounds;
                var broad = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                foreach (var other in broad)
                {
                    if (other.transform.IsChildOf(transform)) continue;
                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, other, other.transform.position, other.transform.rotation, out var normal, out var distance))
                    {
                        netData.position += normal * distance;
                        netData.velocity += normal * Mathf.Max(0f, Vector3.Dot(normal, -netData.velocity));
                    }
                }
            }

            Destroy(collider);
        }

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
            transform.rotation = Quaternion.Euler(0f, orientation.x, 0f);
            
            view.position = Vector3.Lerp(bodyInterpolatePosition1, bodyInterpolatePosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(-orientation.y, orientation.x, 0f);

            if (HasInputAuthority)
            {
                camera.transform.position = view.position;
                camera.transform.rotation = view.rotation;
            }
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

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            var netData = this.netData;
            gameObject.SetActive(true);
            
            netData.position = position;
            netData.velocity = Vector3.zero;
            netData.force = Vector3.zero;
            netData.orientation = new Vector2(rotation.eulerAngles.y, -rotation.eulerAngles.x);
            
            this.netData = netData;
        }

        public struct NetworkData : INetworkStruct
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 force;
            public Vector2 orientation;
            
            public bool jumpFlag;
        }

        public void OffsetRotation(Vector2 delta)
        {
            var data = netData;
            data.orientation += delta;
            netData = data;
        }
    }
}