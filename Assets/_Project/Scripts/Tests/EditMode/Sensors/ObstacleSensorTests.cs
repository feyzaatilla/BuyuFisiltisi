using BuyuFisiltisi.Companion.Sensors;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.Sensors
{
    public class ObstacleSensorTests
    {
        private sealed class FakeRaycaster : IRaycaster
        {
            public float LeftHit = float.PositiveInfinity;
            public float CenterHit = float.PositiveInfinity;
            public float RightHit = float.PositiveInfinity;

            public bool Cast(Vector3 origin, Vector3 direction, float maxDistance, out float hitDistance)
            {
                if (Mathf.Abs(direction.x) < 0.001f)
                    hitDistance = CenterHit;
                else
                    hitDistance = direction.x < 0f ? LeftHit : RightHit;

                return hitDistance <= maxDistance;
            }
        }

        private GameObject _self;

        [SetUp]
        public void SetUp()
        {
            _self = new GameObject("self");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_self);
        }

        private ObstacleSensor Create(FakeRaycaster raycaster) =>
            new ObstacleSensor(_self.transform, raycaster, new ObstacleSensorSettings { Range = 2f, SideAngle = 30f });

        [Test]
        public void NothingInRange_IsClear()
        {
            var sensor = Create(new FakeRaycaster());

            sensor.Sense(0.1f);

            Assert.IsFalse(sensor.Current.Any);
        }

        [Test]
        public void HitOnLeftOnly_ReportsLeft()
        {
            var sensor = Create(new FakeRaycaster { LeftHit = 1f });

            sensor.Sense(0.1f);

            Assert.IsTrue(sensor.Current.Left);
            Assert.IsFalse(sensor.Current.Center);
            Assert.IsFalse(sensor.Current.Right);
            Assert.AreEqual(1f, sensor.Current.NearestDistance, 0.001f);
        }

        [Test]
        public void HitBeyondRange_IsIgnored()
        {
            var sensor = Create(new FakeRaycaster { CenterHit = 5f });

            sensor.Sense(0.1f);

            Assert.IsFalse(sensor.Current.Center);
        }

        [Test]
        public void NearestDistance_PicksClosestHit()
        {
            var sensor = Create(new FakeRaycaster { LeftHit = 1.5f, CenterHit = 0.7f, RightHit = 1.2f });

            sensor.Sense(0.1f);

            Assert.AreEqual(0.7f, sensor.Current.NearestDistance, 0.001f);
        }
    }
}
