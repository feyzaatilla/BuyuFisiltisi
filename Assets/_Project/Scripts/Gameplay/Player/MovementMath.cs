using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    public static class MovementMath
    {
        public static Vector3 CameraRelative(Vector2 input, Vector3 cameraForward)
        {
            var forward = new Vector3(cameraForward.x, 0f, cameraForward.z);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            forward.Normalize();
            var right = new Vector3(forward.z, 0f, -forward.x);
            return Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
        }

        public static float NextVerticalSpeed(float current, bool grounded, float gravity, float deltaTime, float groundStick)
        {
            if (grounded && current <= 0f)
                return -groundStick;

            return current + gravity * deltaTime;
        }

        public static float JumpSpeed(float height, float gravity)
        {
            if (height <= 0f || gravity >= 0f)
                return 0f;

            return Mathf.Sqrt(2f * height * -gravity);
        }
    }
}
