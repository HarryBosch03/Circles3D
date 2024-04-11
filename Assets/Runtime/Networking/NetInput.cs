using Fusion;
using UnityEngine;

namespace Runtime.Networking
{
    public struct NetInput : INetworkInput
    {
        public NetworkButtons buttons;
        public Vector2 movement;
        public Vector2 orientationDelta;
        
        public enum Button
        {
            Jump = 0,
            Run = 1,
            Shoot = 2,
            Aim = 3,
        }
    }
}