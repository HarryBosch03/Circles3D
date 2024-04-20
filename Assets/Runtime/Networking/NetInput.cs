using Fusion;
using UnityEngine;

namespace Circles3D.Runtime.Networking
{
    public struct NetInput : INetworkInput
    {
        public const int Jump = 0;
        public const int Run = 1;
        public const int Shoot = 2;
        public const int Aim = 3;
        public const int Block = 4;
        
        public NetworkButtons buttons;
        public Vector2 movement;
        public Vector2 mouseDelta;
    }
}