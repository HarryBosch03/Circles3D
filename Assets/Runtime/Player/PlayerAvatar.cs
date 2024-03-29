using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerAvatar : NetworkBehaviour
    {
        public float moveSpeed = 10f;
        public float moveAcceleration = 0.1f;
        public float jumpHeight = 2.5f;
        public float gravityScale = 2.5f;
        [Range(0f, 1f)]
        public float airMovementPenalty;

        [Space]
        public float cameraHeight = 1.8f;

        [Space]
        public float mouseSensitivity = 0.3f;

        public Rigidbody body;
        private new Camera camera;
        public Transform view;

        private bool jump;
        public bool onGround;
        private RaycastHit groundHit;

        public Vector2 orientation;

        private Vector3 gravity => Physics.gravity * gravityScale;

        private void Awake()
        {
            camera = Camera.main;
            body = GetComponent<Rigidbody>();

            view = transform.Find("View");
        }

        private void FixedUpdate()
        {
            body.constraints = RigidbodyConstraints.FreezeRotation;

            CheckForGround();
            Move();
            Jump();
            UpdateCamera();

            body.AddForce(gravity - Physics.gravity, ForceMode.Acceleration);

            jump = false;

            PackNetworkData();
        }

        private void UpdateCamera()
        {
            transform.rotation = Quaternion.Euler(0f, orientation.x, 0f);

            view.position = transform.position + Vector3.up * cameraHeight;
            view.rotation = Quaternion.Euler(-orientation.y, orientation.x, 0f);
        }

        private void PackNetworkData()
        {
            if (!IsOwner) return;

            NetworkData data;
            data.position = body.position;
            data.velocity = body.velocity;
            data.orientation = orientation;

            SendDataToServer(data);
        }

        [ServerRpc]
        private void SendDataToServer(NetworkData data)
        {
            UnpackNetworkData(data);
            SendDataToClient(data);
        }

        [ObserversRpc]
        private void SendDataToClient(NetworkData data) => UnpackNetworkData(data);

        public void UnpackNetworkData(NetworkData data)
        {
            if (IsOwner) return;

            body.position = data.position;
            body.velocity = data.velocity;
            orientation = data.orientation;
        }

        private void Jump()
        {
            if (!IsOwner) return;
            if (!jump) return;
            if (!onGround) return;

            var force = Vector3.up * Mathf.Sqrt(2f * jumpHeight * -gravity.y);
            body.AddForce(force, ForceMode.VelocityChange);
        }

        private void Update()
        {
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked;

                if (Keyboard.current.spaceKey.wasPressedThisFrame) jump = true;

                orientation += Mouse.current.delta.ReadValue() * mouseSensitivity;
                orientation.x %= 360f;
                orientation.y = Mathf.Clamp(orientation.y, -90f, 90f);
                
                camera.transform.position = view.position;
                camera.transform.rotation = view.rotation;
            }
        }

        private void CheckForGround()
        {
            var distance = cameraHeight * 0.5f;
            var ray = new Ray(transform.position + Vector3.up * distance, Vector3.down);
            if (Physics.Raycast(ray, out groundHit, distance))
            {
                onGround = true;
                body.position = new Vector3(body.position.x, groundHit.point.y, body.position.z);
                body.velocity = new Vector3(body.velocity.x, Mathf.Max(0f, body.velocity.y), body.velocity.z);

                if (groundHit.rigidbody)
                {
                    groundHit.rigidbody.AddForceAtPosition(gravity * body.mass, groundHit.point);
                }
            }
            else
            {
                onGround = false;
            }
        }

        private void Move()
        {
            if (!IsOwner) return;

            var input = new Vector2()
            {
                x = Keyboard.current.dKey.ReadValue() - Keyboard.current.aKey.ReadValue(),
                y = Keyboard.current.wKey.ReadValue() - Keyboard.current.sKey.ReadValue(),
            };
            input = Vector2.ClampMagnitude(input, 1f);

            var acceleration = 2f / moveAcceleration;
            if (!onGround) acceleration *= 1f - airMovementPenalty;

            var target = transform.TransformDirection(input.x, 0f, input.y) * moveSpeed;
            var force = (target - body.velocity) * acceleration;
            force.y = 0f;

            if (!onGround) force *= input.magnitude;

            body.AddForce(force, ForceMode.Acceleration);
        }

        public struct NetworkData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector2 orientation;
        }
    }
}