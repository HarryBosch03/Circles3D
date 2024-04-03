using FishNet.Object;
using UnityEngine;

namespace Runtime.Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkedBody : NetworkBehaviour
    {
        public float updateFrequency = 0.5f;
        private Rigidbody body;

        private float timer;

        private void Awake() { body = GetComponent<Rigidbody>(); }

        protected override void OnValidate()
        {
            base.OnValidate();
            updateFrequency = Mathf.Min(updateFrequency, 1f / Time.fixedDeltaTime);
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                timer += Time.deltaTime;
                if (timer > 1f / updateFrequency)
                {
                    timer -= 1f / updateFrequency;
                    
                    NetData data;
                    data.position = body.position;
                    data.rotation = body.rotation;
                    data.velocity = body.velocity;
                    data.angularVelocity = body.angularVelocity;
                    SendNetData(data);
                }
            }
        }

        [ObserversRpc(RunLocally = false)]
        private void SendNetData(NetData data)
        {
            if (IsServer) return;

            body.position = data.position;
            body.rotation = data.rotation;
            body.velocity = data.velocity;
            body.angularVelocity = data.angularVelocity;
        }

        private Vector3 ThresholdSwitch(Vector3 local, Vector3 server, float threshold) => (local - server).sqrMagnitude < threshold * threshold ? local : server;
        private Quaternion ThresholdSwitch(Quaternion local, Quaternion server, float threshold) => Quaternion.Angle(local, server) < threshold ? local : server;

        public struct NetData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;
        }
    }
}