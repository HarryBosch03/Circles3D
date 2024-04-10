using Runtime.Util;
using UnityEngine;

namespace Runtime.Weapons
{
    [DisallowMultipleComponent]
    public class GunAnimator : MonoBehaviour
    {
        public Transform target;
        public Vector3 viewPosition = new Vector3(0.09f,-0.03f,0.35f);
        public Vector3 aimPosition = new Vector3(0,0,0.24f);

        [Space]
        public Vector3 localOriginTranslation;

        private Gun gun;
    
        private Vector2 lastCameraRotation;
        private Vector2 weaponSwaySmoothedPosition;
        
        public Vector3 originTranslation => Quaternion.Euler(0f, gun.transform.eulerAngles.y, 0f) * localOriginTranslation;

        private void Awake()
        {
            gun = GetComponentInParent<Gun>();
            if (!target) target = transform;
        }

        private void Update()
        {
            var recoil = gun.recoilData;

            var parent = gun.transform;
            var refPosition = parent.position;
            var refRotation = parent.rotation;
            refPosition += originTranslation;
            
            var localPosition = Vector3.Lerp(viewPosition, aimPosition, gun.aimPercent);
            var localRotation = Quaternion.identity;
            
            localPosition += recoil.position;
            localRotation *= Quaternion.Euler(recoil.rotation);

            if (gun.currentSight)
            {
                localPosition += Vector3.up * gun.currentSight.heightOffset;
            }
            
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
    }
}
