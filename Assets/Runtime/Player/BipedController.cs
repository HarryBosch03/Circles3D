using FMOD.Studio;
using FMODUnity;
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

        [Space]
        public EventReference footstepRef;
        public EventReference landRef;

        private EventInstance footstepSound;
        private EventInstance landSound;
        private bool wasOnGround;
        
        private new Camera camera;
        private RaycastHit groundHit;
        private Vector3 bodyInterpolatePosition0;
        private Vector3 bodyInterpolatePosition1;

        public SimpleKCC kcc { get; private set; }
        
        [Networked] public Vector3 velocity { get; set; }
        [Networked] public float jumpImpulse { get; set; }
        [Networked] public NetInput input { get; set; }
        [Networked] public NetworkButtons prevButtons { get; set; }
        [Networked] public float fieldOfView { get; set; }
        public float cameraDutch { get; set; }
        public Transform view { get; private set; }
        public Vector2 orientation => kcc.GetLookRotation();
        public Vector3 center => kcc.Position + Vector3.up * characterHeight * 0.5f;
        private Vector3 gravity => Physics.gravity * gravityScale;

        private void Awake()
        {
            camera = Camera.main;
            view = transform.Find("View");

            kcc = GetBehaviour<SimpleKCC>();
            
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
            if (GetInput(out NetInput newInput)) input = newInput;
            
            var tangent = Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f);
            kcc.AddLookRotation(input.orientationDelta * tangent * mouseSensitivity);

            Move();
            Jump();
            
            kcc.Move(velocity, jumpImpulse);
            kcc.SetGravity(gravity.y);

            prevButtons = input.buttons;
        }

        private void FixedUpdate()
        {
            bodyInterpolatePosition1 = bodyInterpolatePosition0;
            bodyInterpolatePosition0 = kcc.Position;
        }

        private void Update()
        {
            if (kcc.IsGrounded && !wasOnGround)
            {
                landSound.set3DAttributes(gameObject.To3DAttributes());
                landSound.start();
            }
            
            wasOnGround = kcc.IsGrounded;
            
            footstepSound.set3DAttributes(gameObject.To3DAttributes());
            footstepSound.setParameterByName("MoveSpeed", kcc.IsGrounded ? new Vector2(velocity.x, velocity.z).magnitude / runSpeed : 0f);
        }

        private void Jump()
        {
            jumpImpulse = 0f;
            
            var jump = input.buttons.WasPressed(prevButtons, InputButton.Jump);
            if (jump && kcc.IsGrounded)
            {
                jumpImpulse = Mathf.Sqrt(2f * jumpHeight * -gravity.y) * kcc.Rigidbody.mass;
            }
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.Euler(0f, orientation.y, 0f);

            view.position = Vector3.Lerp(bodyInterpolatePosition1, bodyInterpolatePosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime) + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(kcc.GetLookRotation()) * Quaternion.Euler(0f, 0f, cameraDutch);
        }

        private void Move()
        {
            var moveInput = Vector2.ClampMagnitude(input.movement, 1f);

            var acceleration = 2f / moveAcceleration;
            if (!kcc.IsGrounded) acceleration *= 1f - airMovementPenalty;
            
            var orientation = Quaternion.Euler(0f, kcc.GetLookRotation().y, 0f);

            var moveSpeed = input.buttons.IsSet(InputButton.Run) ? runSpeed : walkSpeed;
            var target = orientation * new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            var force = (target - velocity) * acceleration;
            force.y = 0f;

            if (!kcc.IsGrounded) force *= moveInput.magnitude;

            velocity += force * Runner.DeltaTime;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(true);

            kcc.SetPosition(position);
            velocity = Vector3.zero;
            kcc.SetLookRotation(new Vector2(rotation.y, rotation.x));
        }

        public void OffsetRotation(Vector2 delta) => kcc.AddLookRotation(delta);

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            enabled = true;
            kcc.SetPosition(position);
            kcc.SetLookRotation(rotation);
        }
    }
}