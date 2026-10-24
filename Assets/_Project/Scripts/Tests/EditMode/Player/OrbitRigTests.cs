using BuyuFisiltisi.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.Player
{
    public class OrbitRigTests
    {
        private static OrbitRig CreateRig() => new OrbitRig(0f, 20f, 6f, -10f, 70f, 2f, 12f);

        [Test]
        public void Pitch_StaysInsideLimits()
        {
            var rig = CreateRig();

            rig.Rotate(0f, 500f);
            Assert.AreEqual(70f, rig.Pitch, 0.001f);

            rig.Rotate(0f, -500f);
            Assert.AreEqual(-10f, rig.Pitch, 0.001f);
        }

        [Test]
        public void Yaw_WrapsAround()
        {
            var rig = CreateRig();

            rig.Rotate(370f, 0f);

            Assert.AreEqual(10f, rig.Yaw, 0.001f);
        }

        [Test]
        public void Zoom_StaysInsideLimits()
        {
            var rig = CreateRig();

            rig.Zoom(100f);
            Assert.AreEqual(2f, rig.Distance, 0.001f);

            rig.Zoom(-100f);
            Assert.AreEqual(12f, rig.Distance, 0.001f);
        }

        [Test]
        public void Offset_IsBehindTarget()
        {
            var rig = new OrbitRig(0f, 0f, 5f, -10f, 70f, 2f, 12f);

            Assert.That(Vector3.Distance(rig.Offset, new Vector3(0f, 0f, -5f)), Is.LessThan(0.001f));
        }
    }
}
