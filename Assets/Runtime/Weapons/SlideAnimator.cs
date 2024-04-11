using UnityEngine;

namespace Circles3D.Runtime.Weapons
{
    public class SlideAnimator : MonoBehaviour
    {
        public Vector3 offset;
        public Vector3 position0;
        public Vector3 position1;
        public AnimationCurve curve;
        public float duration;

        private Gun gun;

        private void Awake()
        {
            gun = GetComponentInParent<Gun>();
        }

        private void Update()
        {
            var t = Mathf.Clamp01((Time.time - gun.lastShootTime) / duration);
            transform.localPosition = offset + Vector3.Lerp(position0, position1, curve.Evaluate(t / duration));
        }
    }
}
