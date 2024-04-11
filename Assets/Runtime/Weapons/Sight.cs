using UnityEngine;

namespace Runtime.Weapons
{
    public class Sight : MonoBehaviour
    {
        public float zoomLevel;
        public Transform sightCenter;
        public float viewportFov;
        public float zOffset;

        private Gun gun;
    
        public float heightOffset => sightCenter ? -sightCenter.localPosition.y : 0f;

        private void OnEnable()
        {
            gun = GetComponentInParent<Gun>();
            if (gun) gun.currentSight = this;
        }

        private void OnDisable()
        {
            if (gun && gun.currentSight == this) gun.currentSight = null;
        }
    }
}
