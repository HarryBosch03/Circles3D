using System;
using Runtime.Player;
using UnityEngine;

namespace Runtime.Animation
{
    public class ArmAnimation : MonoBehaviour
    {
        public Chirality chirality;
        public Vector3 hintOrientation;
        public Vector3 correctiveRotation;

        private float length0;
        private float length1;
        
        private PlayerAvatar avatar;
        private Transform mid;
        private Transform tip;

        public Vector3 hint => transform.parent.rotation * Quaternion.Euler(hintOrientation) * Vector3.up;

        private void Awake()
        {
            avatar = GetComponentInParent<PlayerAvatar>();
            
            mid = transform.GetChild(0);
            tip = mid.GetChild(0);

            length0 = mid.localPosition.magnitude;
            length1 = tip.localPosition.magnitude;
        }

        private void LateUpdate()
        {
            Solve();
        }

        private void Solve()
        {
            if (!avatar) return;
            if (!avatar.gun) return;

            var target = chirality switch {
                Chirality.Left => avatar.gun.leftHandHold,
                Chirality.Right => avatar.gun.rightHandHold,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (!target) return;

            var start = transform.position;
            var end = target.position;

            var length2 = (end - start).magnitude;
            var a0 = Mathf.Acos((sqr(length0) + sqr(length2) - sqr(length1)) / (2 * length0 * length2)) * Mathf.Rad2Deg;

            var correctiveRotation = Quaternion.Euler(this.correctiveRotation);
            
            transform.rotation = Quaternion.LookRotation(end - start, hint) * Quaternion.Euler(a0, 0f, 0f) * correctiveRotation;
            mid.rotation = Quaternion.LookRotation(end - mid.position, hint) * correctiveRotation;
            tip.rotation = target.rotation;

            float sqr(float x) => x * x;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, hint);
        }

        public enum Chirality
        {
            Left,
            Right,
        }
    }
}