using FishNet.Object;

namespace Runtime.Networking
{
    public class SidedObject : NetworkBehaviour
    {
        public Condition withOwnership;

        public override void OnStartNetwork()
        {
            if (withOwnership != Condition.DontCare)
            {
                gameObject.SetActive(withOwnership == Condition.Active == Owner.IsLocalClient);
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