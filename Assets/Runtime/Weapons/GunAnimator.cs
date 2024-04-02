using Runtime.Player;
using UnityEngine;

namespace Runtime.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Gun))]
    public class GunAnimator : MonoBehaviour
    {
        [Space]
        public Vector3 fpViewPosition = new Vector3(0.09f,-0.03f,0.35f);
        public Vector3 fpAimPosition = new Vector3(0,0,0.24f);
        public Vector3 tpViewPosition;
        public Vector3 tpAimPosition;

        [Space]
        public Transform thirdPersonParent;

        private Gun gun;
        private Transform model;
        private Camera mainCam;
    
        private Vector2 lastCameraRotation;
        private Vector2 weaponSwaySmoothedPosition;

        private void Awake()
        {
            mainCam = Camera.main;
            gun = GetComponent<Gun>();
            model = transform.Find("Model");
            if (!model) model = transform;
        }

        private void Update()
        {
            var recoil = gun.recoilData;

            var isFirstPerson = (mainCam.transform.position - transform.position).magnitude < 0.04f;
            var viewPosition = isFirstPerson ? fpViewPosition : tpViewPosition;
            var aimPosition = isFirstPerson ? fpAimPosition : tpAimPosition;

            var parent = isFirstPerson ? model.transform.parent : thirdPersonParent;

            var localPosition = Vector3.Lerp(viewPosition, aimPosition, gun.aimPercent);
            var localRotation = Quaternion.identity;
            
            localPosition += recoil.position;
            localRotation *= Quaternion.Euler(recoil.rotation);
            
            model.position = parent.TransformPoint(localPosition);
            model.rotation = parent.rotation * localRotation;
        }
    }
}
