using BuyuFisiltisi.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.Player
{
    public class MovementMathTests
    {
        [Test]
        public void ForwardInput_FollowsCameraForward()
        {
            var direction = MovementMath.CameraRelative(Vector2.up, Vector3.forward);

            Assert.That(Vector3.Distance(direction, Vector3.forward), Is.LessThan(0.001f));
        }

        [Test]
        public void RightInput_IsCameraRight()
        {
            var direction = MovementMath.CameraRelative(Vector2.right, Vector3.right);

            Assert.That(Vector3.Distance(direction, Vector3.back), Is.LessThan(0.001f));
        }

        [Test]
        public void TiltedCamera_StaysOnGround()
        {
            var direction = MovementMath.CameraRelative(Vector2.up, new Vector3(0f, -1f, 1f));

            Assert.AreEqual(0f, direction.y, 0.001f);
            Assert.AreEqual(1f, direction.magnitude, 0.001f);
        }

        [Test]
        public void DiagonalInput_IsNotFaster()
        {
            var direction = MovementMath.CameraRelative(new Vector2(1f, 1f), Vector3.forward);

            Assert.That(direction.magnitude, Is.LessThanOrEqualTo(1.0001f));
        }

        [Test]
        public void Grounded_KeepsSmallDownwardSpeed()
        {
            var speed = MovementMath.NextVerticalSpeed(-5f, true, -20f, 0.1f, 2f);

            Assert.AreEqual(-2f, speed, 0.001f);
        }

        [Test]
        public void JumpSpeed_ReachesRequestedHeight()
        {
            var speed = MovementMath.JumpSpeed(0.7f, -20f);
            var peak = speed * speed / (2f * 20f);

            Assert.AreEqual(0.7f, peak, 0.001f);
        }

        [Test]
        public void JumpSpeed_IsZeroForInvalidValues()
        {
            Assert.AreEqual(0f, MovementMath.JumpSpeed(0f, -20f));
            Assert.AreEqual(0f, MovementMath.JumpSpeed(1f, 0f));
        }

        [Test]
        public void Airborne_AccumulatesGravity()
        {
            var speed = MovementMath.NextVerticalSpeed(0f, false, -20f, 0.1f, 2f);

            Assert.AreEqual(-2f, speed, 0.001f);
        }
    }
}
