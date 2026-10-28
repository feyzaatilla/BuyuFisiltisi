using System;
using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakTrustTests
    {
        [Test]
        public void NewTrust_StartsAtZero_DoorClosed()
        {
            var trust = new OakTrust();

            Assert.AreEqual(0, trust.Current);
            Assert.IsFalse(trust.CanOpenDoor);
            Assert.IsFalse(trust.MayAttack);
        }

        [Test]
        public void Apply_ClampsToLimits_AndReturnsRealChange()
        {
            var trust = new OakTrust(start: 4);

            Assert.AreEqual(1, trust.Apply(2));
            Assert.AreEqual(OakTrust.DefaultMax, trust.Current);
            Assert.AreEqual(0, trust.Apply(2));
        }

        [Test]
        public void ReachingThreshold_OpensDoor()
        {
            var trust = new OakTrust();

            trust.Apply(2);
            Assert.IsFalse(trust.CanOpenDoor);

            trust.Apply(1);
            Assert.IsTrue(trust.CanOpenDoor);
        }

        [Test]
        public void LowTrust_AllowsAttack()
        {
            var trust = new OakTrust(start: -2);

            Assert.IsFalse(trust.MayAttack);
            trust.Apply(-1);
            Assert.IsTrue(trust.MayAttack);
        }

        [Test]
        public void InvalidLimits_Throw()
        {
            Assert.Throws<ArgumentException>(() => new OakTrust(min: 3, max: -3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OakTrust(doorThreshold: 9));
        }
    }
}
