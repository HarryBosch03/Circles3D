using Runtime.Player;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("aimMovementReduction")]
        [Space]
        [Range(0f, 1f)]
        public float aimReduction;

        private Vector2 lastTarget;
        private Vector2 position;

        private float distance;
        private float groundedBlend;

        private PlayerAvatar player;

        private void Awake() { player = GetComponentInParent<PlayerAvatar>(); }

        private void LateUpdate()
        {
            groundedBlend = Mathf.MoveTowards(groundedBlend, player.movement.kcc.IsGrounded ? 1f : 0f, Time.deltaTime * 10f);

            ApplySmoothing();
            ApplyMovement();
            ApplyAiming();
        }

        private void ApplyAiming()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition * aimReduction, player.gun.aimPercent);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.SlerpUnclamped(Quaternion.identity, transform.localRotation, aimReduction), player.gun.aimPercent);
        }

        private void ApplyMovement()
        {
            if (!player.Runner) return;
            
            var velocity = Vector3.Lerp(Vector3.zero, player.movement.velocity, groundedBlend);
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