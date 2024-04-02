using System;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Animation
{
    public class GunModel3rdPerson : MonoBehaviour
    {
        public Vector3 hipOffset;
        public Vector3 aimOffset;
        
        public Transform torso;
        
        private Gun gun;
        private Transform model;

        private void Awake()
        {
            gun = GetComponentInParent<Gun>();
            model = gun.transform.Find("Model");
        }

        private void LateUpdate()
        {
            transform.position = torso.TransformPoint(model.localPosition + hipOffset);
            transform.localRotation = model.localRotation;
        }
    }
}