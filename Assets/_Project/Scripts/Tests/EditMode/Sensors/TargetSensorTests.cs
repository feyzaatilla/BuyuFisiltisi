using BuyuFisiltisi.Companion.Sensors;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.Sensors
{
    public class TargetSensorTests
    {
        [Test]
        public void TargetAhead_HasZeroAngleAndFlatDistance()
        {
            var reading = TargetSensor.Evaluate(Vector3.zero, Vector3.forward, new Vector3(0f, 5f, 4f), 0.5f);

            Assert.IsTrue(reading.HasTarget);
            Assert.AreEqual(4f, reading.Distance, 0.001f);
            Assert.AreEqual(0f, reading.Angle, 0.001f);
            Assert.IsFalse(reading.Arrived);
        }

        [Test]
        public void TargetOnRight_HasPositiveAngle()
        {
            var reading = TargetSensor.Evaluate(Vector3.zero, Vector3.forward, new Vector3(3f, 0f, 0f), 0.5f);

            Assert.AreEqual(90f, reading.Angle, 0.001f);
        }

        [Test]
        public void TargetWithinArriveDistance_ReportsArrived()
        {
            var reading = TargetSensor.Evaluate(Vector3.zero, Vector3.forward, new Vector3(0.2f, 0f, 0.2f), 0.5f);

            Assert.IsTrue(reading.Arrived);
        }

        [Test]
        public void NoTarget_ReportsNone()
        {
            var self = new GameObject("self");
            try
            {
                var sensor = new TargetSensor(self.transform, 0.5f);
                sensor.Sense(0.1f);

                Assert.IsFalse(sensor.Current.HasTarget);
            }
            finally
            {
                Object.DestroyImmediate(self);
            }
        }
    }
}
