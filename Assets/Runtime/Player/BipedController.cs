using Fusion;
using Fusion.Addons.SimpleKCC;
using Runtime.Networking;
using UnityEngine;

namespace Runtime.Player
{
    [SelectionBase]
    [RequireComponent(typeof(SimpleKCC))]
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

        private new Camera camera;
        private RaycastHit groundHit;
        private SimpleKCC kcc;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;

        [Networked]
        public NetworkData netData { get; set; }

        [Networked]
        public NetInput input { get; set; }
        public Transform view { get; private set; }
        public Vector2 orientation => kcc.GetLookRotation();
        public Vector3 center => kcc.Position + Vector3.up * characterHeight * 0.5f;
        private Vector3 gravity => Physics.gravity * gravityScale;

        private void Awake()
        {
            camera = Camera.main;
            view = transform.Find("View");

            kcc = GetBehaviour<SimpleKCC>();
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
            kcc.AddLookRotation(input.orientationDelta * tangent * mouseSensitivity);

            Move(ref netData);
            Jump(ref netData);
            
            kcc.Move(netData.velocity, netData.jumpImpulse);
            kcc.SetGravity(gravity.y);
            
            this.netData = netData;
        }

        private void FixedUpdate()
        {
            bodyInterpolatePosition1 = bodyInterpolatePosition0;
            bodyInterpolatePosition0 = kcc.Position;
        }

        private void Jump(ref NetworkData netData)
        {
            netData.jumpImpulse = 0f;
            
            var jump = input.buttons.IsSet(InputButton.Jump);
            if (jump && !netData.jumpFlag && kcc.IsGrounded)
            {
                netData.jumpImpulse = Mathf.Sqrt(2f * jumpHeight * -gravity.y) * kcc.Rigidbody.mass;
            }

            netData.jumpFlag = jump;
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(0f, orientation.y, 0f);

            view.position = Vector3.Lerp(bodyInterpolatePosition1, bodyInterpolatePosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(orientation);

            if (HasInputAuthority)
            {
                camera.transform.position = view.position;
                camera.transform.rotation = view.rotation;
            }
        }

        private void Move(ref NetworkData netData)
        {
            var moveInput = Vector2.ClampMagnitude(input.movement, 1f);

            var acceleration = 2f / moveAcceleration;
            if (!kcc.IsGrounded) acceleration *= 1f - airMovementPenalty;
            
            var orientation = Quaternion.Euler(0f, kcc.GetLookRotation().y, 0f);

            var moveSpeed = input.buttons.IsSet(InputButton.Run) ? runSpeed : walkSpeed;
            var target = orientation * new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            var force = (target - netData.velocity) * acceleration;
            force.y = 0f;

            if (!kcc.IsGrounded) force *= moveInput.magnitude;

            netData.velocity += force * Runner.DeltaTime;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            var netData = this.netData;
            gameObject.SetActive(true);

            kcc.SetPosition(position);
            netData.velocity = Vector3.zero;
            kcc.SetLookRotation(new Vector2(rotation.y, rotation.x));

            this.netData = netData;
        }

        public struct NetworkData : INetworkStruct
        {
            public Vector3 velocity;
            public float jumpImpulse;

            public bool jumpFlag;
        }

        public void OffsetRotation(Vector2 delta) { }
    }
}