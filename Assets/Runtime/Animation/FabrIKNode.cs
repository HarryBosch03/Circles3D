using UnityEngine;

namespace Runtime.Animation
{
    public class FabrIKNode : MonoBehaviour
    {
        private FabrIKNode next;
        private FabrIKNode previous;

        protected virtual void Awake()
        {
            foreach (Transform child in transform)
            {
                next = child.GetComponent<FabrIKNode>();
                if (next) break;
            }

            previous = transform.parent.GetComponent<FabrIKNode>();
        }

        protected void ConstrainToNext()
        {
            
        }

        protected void ConstrainToPrevious()
        {
            
        }
    }
}