using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Player
{
    public class PlayerInstance : NetworkBehaviour
    {
        public float mouseSensitivity = 0.3f;

        [Space]
        public PlayerAvatar avatar;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!avatar) avatar = GetComponentInChildren<PlayerAvatar>();
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            if (IsOwner)
            {
                if (avatar)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    avatar.owningPlayerInstance = this;
                    
                    PlayerAvatar.InputData input;
                    var kb = Keyboard.current;
                    var m = Mouse.current;

                    input.movement.x = kb.dKey.ReadValue() - kb.aKey.ReadValue();
                    input.movement.y = kb.wKey.ReadValue() - kb.sKey.ReadValue();

                    input.lookDelta = m.delta.ReadValue() * mouseSensitivity;

                    input.run = kb.leftShiftKey.isPressed;
                    input.jump = kb.spaceKey.isPressed;

                    input.shoot = m.leftButton.isPressed;
                    input.aim = m.rightButton.isPressed;

                    avatar.input = input;
                }
            }
        }
    }
}