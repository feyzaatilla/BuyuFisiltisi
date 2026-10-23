using BuyuFisiltisi.Gameplay.World;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class SwitchLatchTests
    {
        [Test]
        public void NewLatch_IsNotPulled()
        {
            var latch = new SwitchLatch("bridge_switch");

            Assert.IsFalse(latch.IsPulled);
            Assert.AreEqual("bridge_switch", latch.Id);
        }

        [Test]
        public void FirstPull_Succeeds_SecondPull_Fails()
        {
            var latch = new SwitchLatch("bridge_switch");

            Assert.IsTrue(latch.TryPull());
            Assert.IsFalse(latch.TryPull());
            Assert.IsTrue(latch.IsPulled);
        }

        [Test]
        public void Reset_AllowsPullingAgain()
        {
            var latch = new SwitchLatch("bridge_switch");
            latch.TryPull();

            latch.Reset();

            Assert.IsFalse(latch.IsPulled);
            Assert.IsTrue(latch.TryPull());
        }
    }
}
