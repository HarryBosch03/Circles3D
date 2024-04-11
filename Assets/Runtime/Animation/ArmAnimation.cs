using System;
using Circles3D.Runtime.Player;
using UnityEngine;

namespace Circles3D.Runtime.Animation
{
    public class ArmAnimation : MonoBehaviour
    {
        public Chirality chirality;
        public Perspective perspective;
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

        private void LateUpdate() { Solve(); }

        private void Solve()
        {
            if (!avatar) return;
            if (!avatar.gun) return;

            var target = GetTarget();

            if (!target) return;

            var start = transform.position;
            var end = target.position;

            var length2 = (end - start).magnitude;
            var a0 = Mathf.Acos(Mathf.Clamp((sqr(length0) + sqr(length2) - sqr(length1)) / (2 * length0 * length2), -1f, 1f)) * Mathf.Rad2Deg;

            var correctiveRotation = Quaternion.Euler(this.correctiveRotation);

            transform.rotation = Quaternion.LookRotation(end - start, hint) * Quaternion.Euler(a0, 0f, 0f) * correctiveRotation;
            mid.rotation = Quaternion.LookRotation(end - mid.position, hint) * correctiveRotation;
            tip.rotation = target.rotation * correctiveRotation;

            float sqr(float x) => x * x;
        }

        private Transform GetTarget()
        {
            var modelData = perspective switch
            {
                Perspective.FirstPerson => avatar.gun.modelDataFirstPerson,
                Perspective.ThirdPerson => avatar.gun.modelDataThirdPerson,
                _ => throw new ArgumentOutOfRangeException()
            };

            return chirality switch
            {
                Chirality.Left => modelData.leftHandTarget,
                Chirality.Right => modelData.rightHandTarget,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, hint);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                var target = GetTarget();
                if (target)
                {
                    Gizmos.DrawSphere(target.position, 0.1f);
                }
            }
        }

        public enum Chirality
        {
            Left,
            Right,
        }

        public enum Perspective
        {
            FirstPerson,
            ThirdPerson
        }
    }
}