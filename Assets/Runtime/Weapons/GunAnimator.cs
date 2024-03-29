using Runtime.Player;
using UnityEngine;

namespace Runtime.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Gun))]
    public class GunAnimator : MonoBehaviour
    {
        [Space]
        public Vector3 viewPosition = new Vector3(0.09f,-0.04f,0.28f);
        public Vector3 aimPosition = new Vector3(0,0,0.23f);

        private Gun gun;
    
        private Vector2 lastCameraRotation;
        private Vector2 weaponSwaySmoothedPosition;

        public PlayerAvatar player => gun.player;
    
        private void Awake()
        {
            gun = GetComponent<Gun>();
        }

        private void FixedUpdate()
        {
            var recoil = gun.recoilData;
        
            transform.localPosition = Vector3.Lerp(viewPosition, aimPosition, gun.aimPercent);
            transform.localRotation = Quaternion.identity;
     
            transform.localPosition += recoil.position;
            transform.localRotation *= Quaternion.Euler(recoil.rotation);
        }
    }
}
