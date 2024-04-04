using System;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Networking
{
    public class SidedObject : NetworkBehaviour
    {
        [FormerlySerializedAs("withOwnership")] public Condition withInputAuthority;

        public override void Spawned()
        {
            if (withInputAuthority != Condition.DontCare)
            {
                gameObject.SetActive(HasInputAuthority == (withInputAuthority == Condition.Active));
            }
        }

        public enum Condition
        {
            DontCare,
            Inactive,
            Active,
        }
    }
}