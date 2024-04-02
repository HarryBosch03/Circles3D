using System;
using FishNet.Object;
using UnityEngine;

namespace Runtime.Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkedBody : NetworkBehaviour
    {
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                NetData data;
                data.position = body.position;
                data.rotation = body.rotation;
                data.velocity = body.velocity;
                data.angularVelocity = body.angularVelocity;
                SendNetData(data);
            }
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = false)]
        private void SendNetData(NetData data)
        {
            body.position = data.position;
            body.rotation = data.rotation;
            body.velocity = data.velocity;
            body.angularVelocity = data.angularVelocity;
        }

        public struct NetData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;
        }
    }
}