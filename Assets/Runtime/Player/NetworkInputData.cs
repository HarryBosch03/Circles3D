using Fusion;
using UnityEngine;

namespace Runtime.Player
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 movement;
        public bool run;
        public bool jump;
        public bool shoot;
        public bool aim;
    }
}