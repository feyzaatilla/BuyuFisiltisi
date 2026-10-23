using BuyuFisiltisi.Companion.Sensors;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Sensors
{
    public class StuckDetectorTests
    {
        private static StuckDetectorSettings Settings() =>
            new StuckDetectorSettings { SpeedThreshold = 0.05f, TimeThreshold = 1f };

        [Test]
        public void SlowLongEnough_ReportsStuck()
        {
            var speed = 0f;
            var detector = new StuckDetector(() => speed, Settings());

            detector.Sense(0.6f);
            Assert.IsFalse(detector.Current);

            detector.Sense(0.6f);
            Assert.IsTrue(detector.Current);
        }

        [Test]
        public void MovingAgain_ClearsStuck()
        {
            var speed = 0f;
            var detector = new StuckDetector(() => speed, Settings());
            detector.Sense(2f);
            Assert.IsTrue(detector.Current);

            speed = 1f;
            detector.Sense(0.1f);

            Assert.IsFalse(detector.Current);
            Assert.AreEqual(0f, detector.SlowTime);
        }

        [Test]
        public void Reset_ClearsState()
        {
            var detector = new StuckDetector(() => 0f, Settings());
            detector.Sense(2f);

            detector.Reset();

            Assert.IsFalse(detector.Current);
            Assert.AreEqual(0f, detector.SlowTime);
        }
    }
}
