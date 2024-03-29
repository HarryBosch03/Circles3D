using Runtime.Player;
using UnityEngine;

namespace Runtime.Weapons
{
    public class ViewAnimator : MonoBehaviour
    {
        public float translationSway;
        public float rotationSway;
        public float smoothing;

        [Space]
        public Vector2 moveAmplitude;
        public float moveFrequency;

        private Vector2 lastTarget;
        private Vector2 position;

        private float distance;

        private PlayerAvatar player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerAvatar>();
        }

        private void FixedUpdate()
        {
            ApplySmoothing();
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            var velocity = player.onGround ? player.body.velocity : Vector3.zero;
            var speed = new Vector2(velocity.x, velocity.z).magnitude;
        
            distance += speed * moveFrequency * Time.deltaTime;

            transform.localPosition += new Vector3
            {
                x = Mathf.Sin(distance) * moveAmplitude.x,
                y = Mathf.Sin(2f * distance) * 0.5f * moveAmplitude.y,
            } * speed;
        }

        private void ApplySmoothing()
        {
            var target = new Vector2(-transform.eulerAngles.y, transform.eulerAngles.x);
            var delta = new Vector2
            {
                x = Mathf.DeltaAngle(lastTarget.x, target.x),
                y = Mathf.DeltaAngle(lastTarget.y, target.y),
            };

            position = Vector2.Lerp(position, delta, Time.deltaTime / Mathf.Max(Time.deltaTime, smoothing));

            transform.localPosition = new Vector3(position.x, position.y, 0f) * translationSway;
            transform.localRotation = Quaternion.Euler(new Vector3(-position.y, position.x) * rotationSway);
        
            lastTarget = target;
        }
    }
}
