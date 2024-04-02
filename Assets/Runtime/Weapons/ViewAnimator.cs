using Runtime.Player;
using UnityEngine;

namespace Runtime.Weapons
{
    public class ViewAnimator : MonoBehaviour
    {
        public float translationSway;
        public float rotationSway;
        public float smoothing;
        public float clamp;

        [Space]
        public Vector2 moveAmplitude;
        public float moveFrequency;

        private Vector2 lastTarget;
        private Vector2 position;

        private float distance;
        private float groundedBlend;
        
        private PlayerAvatar player;

        private void Awake()
        {
            player = GetComponentInParent<PlayerAvatar>();
        }

        private void FixedUpdate()
        {
            groundedBlend = Mathf.MoveTowards(groundedBlend, player.onGround ? 1f : 0f, Time.deltaTime * 10f);
            
            ApplySmoothing();
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            var velocity = Vector3.Lerp(Vector3.zero, player.body.velocity, groundedBlend);
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
            position = Vector2.ClampMagnitude(position, clamp);
            
            transform.localPosition = new Vector3(position.x, position.y, 0f) * translationSway;
            transform.localRotation = Quaternion.Euler(new Vector3(-position.y, position.x) * rotationSway);
        
            lastTarget = target;
        }
    }
}
