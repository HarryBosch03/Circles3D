using Circles3D.Runtime.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Circles3D.Runtime.Animation
{
    public class BodyAnimator : MonoBehaviour
    {
        public float torsoTwist;
    
        [Space]
        public Transform head;
        public Transform torso;
        public Transform root;
        public Renderer bodyRenderer;
        public Renderer armsRenderer;

        private PlayerAvatar player;

        private void OnValidate()
        {
            if (!bodyRenderer) bodyRenderer = GetComponentInChildren<Renderer>();
        }

        private void Awake()
        {
            player = GetComponentInParent<PlayerAvatar>();
        }

        private void Update()
        {
            if (!player.Object) return;

            bodyRenderer.enabled = !player.activeViewer;
            armsRenderer.enabled = player.activeViewer && player.health.alive;
            
            var twist = Quaternion.Euler(90f + torsoTwist, 90f, 90f);
            root.localRotation = twist;
            
            var view = player ? player.movement.orientation : default;

            torso.localRotation = Quaternion.Euler(view.x * 0.5f, 0f, 0f);
            head.rotation = Quaternion.Euler(view.x, view.y, 0f);
        }
    }
}
