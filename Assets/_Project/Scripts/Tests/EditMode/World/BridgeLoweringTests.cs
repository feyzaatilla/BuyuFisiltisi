using BuyuFisiltisi.Gameplay.World;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class BridgeLoweringTests
    {
        [Test]
        public void BeforeBegin_StaysRaised()
        {
            var lowering = new BridgeLowering(90f, 0f, 2f);

            Assert.IsFalse(lowering.Advance(1f));
            Assert.AreEqual(90f, lowering.CurrentAngle, 0.001f);
        }

        [Test]
        public void HalfWay_IsBetweenRaisedAndLowered()
        {
            var lowering = new BridgeLowering(90f, 0f, 2f);
            lowering.Begin();

            lowering.Advance(1f);

            Assert.AreEqual(0.5f, lowering.Progress, 0.001f);
            Assert.Less(lowering.CurrentAngle, 90f);
            Assert.Greater(lowering.CurrentAngle, 0f);
        }

        [Test]
        public void AfterDuration_IsLoweredAndFinished()
        {
            var lowering = new BridgeLowering(90f, 0f, 2f);
            lowering.Begin();

            Assert.IsFalse(lowering.Advance(1.5f));
            Assert.IsTrue(lowering.Advance(0.6f));
            Assert.IsTrue(lowering.IsFinished);
            Assert.AreEqual(0f, lowering.CurrentAngle, 0.001f);
        }

        [Test]
        public void Begin_WorksOnlyOnce()
        {
            var lowering = new BridgeLowering(90f, 0f, 2f);

            Assert.IsTrue(lowering.Begin());
            Assert.IsFalse(lowering.Begin());
            lowering.Advance(5f);
            Assert.IsFalse(lowering.Begin());
        }
    }
}
