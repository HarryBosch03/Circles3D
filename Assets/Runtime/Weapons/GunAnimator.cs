using System;
using Circles3D.Runtime.Util;
using UnityEngine;

namespace Circles3D.Runtime.Weapons
{
    [DisallowMultipleComponent]
    public class GunAnimator : MonoBehaviour
    {
        public Transform target;
        public Transform pivot;
        public Vector3 viewPosition;
        public Vector3 viewRotation;
        public Vector3 aimPosition;

        [Space]
        public Vector3 parentOriginTranslation;

        private Gun gun;
    
        private Vector2 lastCameraRotation;
        private Vector2 weaponSwaySmoothedPosition;
        
        public Vector3 originTranslation => Quaternion.Euler(0f, gun.transform.eulerAngles.y, 0f) * parentOriginTranslation;

        private void Awake()
        {
            gun = GetComponentInParent<Gun>();
            if (!target) target = transform;
        }

        private void Update()
        {
            if (!target) return;
            
            var recoil = gun.recoilData;
            var parent = gun.transform;
            
            var pivot = this.pivot ? target.InverseTransformPoint(this.pivot.position) : Vector3.zero;
            var refPosition = parent.position;
            var refRotation = parent.rotation;
            refPosition += originTranslation;
            
            var localPosition = Vector3.Lerp(viewPosition / 100f, aimPosition / 100f, gun.aimPercent);
            var localRotation = Quaternion.Slerp(Quaternion.Euler(viewRotation), Quaternion.identity, gun.aimPercent);
            
            
            localPosition += recoil.position;
            localRotation *= Quaternion.Euler(recoil.rotation);

            if (gun.currentSight)
            {
                localPosition += Vector3.up * gun.currentSight.heightOffset;
            }

            localPosition += pivot - localRotation * pivot;
            
            target.position = refRotation * localPosition + refPosition;
            target.rotation = refRotation * localRotation;
        }

        private void OnDrawGizmosSelected()
        {
            gun = GetComponentInParent<Gun>();
            if (!gun) return;
            
            var parent = gun.transform;
            MoreGizmos.DrawAxis(parent.position + originTranslation, parent.rotation, 0.04f, 0.2f);
        }

        [Serializable]
        public class Pose
        {
            public Vector3 position;
            public Vector3 rotation;
        }
    }
}
