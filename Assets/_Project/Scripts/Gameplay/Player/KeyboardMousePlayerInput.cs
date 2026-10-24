using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Player
{
    public sealed class KeyboardMousePlayerInput : IPlayerInput
    {
        public Vector2 Move
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return Vector2.zero;

                var value = Vector2.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
                return Vector2.ClampMagnitude(value, 1f);
            }
        }

        public bool Sprint => Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        public bool JumpPressed => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        public Vector2 Look => Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;

        public bool IsLooking => Mouse.current != null && Mouse.current.rightButton.isPressed;

        public float Zoom => Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
    }
}
