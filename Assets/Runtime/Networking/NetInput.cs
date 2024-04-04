using Fusion;
using UnityEngine;

namespace Runtime.Networking
{
    public enum InputButton
    {
        Jump = 0,
        Run = 1,
        Shoot = 2,
        Aim = 3,
    }
    
    public struct NetInput : INetworkInput
    {
        public NetworkButtons buttons;
        public Vector2 movement;
        public Vector2 mouseDelta;
    }
}