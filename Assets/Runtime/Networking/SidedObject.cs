using UnityEngine;

namespace Runtime.Networking
{
    public class SidedObject : MonoBehaviour
    {
        public Condition withOwnership;

        public void Start()
        {
            if (withOwnership != Condition.DontCare)
            {
                gameObject.SetActive(withOwnership == Condition.Active);
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