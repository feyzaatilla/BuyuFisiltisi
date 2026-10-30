using BuyuFisiltisi.Gameplay.World;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class DoorLatchTests
    {
        [Test]
        public void NewLatch_IsClosed()
        {
            Assert.IsFalse(new DoorLatch("MuhurluKapi").IsOpen);
        }

        [Test]
        public void TryOpen_WithMatchingId_Opens()
        {
            var latch = new DoorLatch("MuhurluKapi");

            Assert.IsTrue(latch.TryOpen("MuhurluKapi"));
            Assert.IsTrue(latch.IsOpen);
        }

        [Test]
        public void TryOpen_WithOtherId_StaysClosed()
        {
            var latch = new DoorLatch("MuhurluKapi");

            Assert.IsFalse(latch.TryOpen("BaskaKapi"));
            Assert.IsFalse(latch.IsOpen);
        }

        [Test]
        public void TryOpen_WithNullId_StaysClosed()
        {
            var latch = new DoorLatch("MuhurluKapi");

            Assert.IsFalse(latch.TryOpen(null));
            Assert.IsFalse(latch.IsOpen);
        }

        [Test]
        public void TryOpen_Twice_OpensOnlyOnce()
        {
            var latch = new DoorLatch("MuhurluKapi");
            latch.TryOpen("MuhurluKapi");

            Assert.IsFalse(latch.TryOpen("MuhurluKapi"));
        }
    }
}
