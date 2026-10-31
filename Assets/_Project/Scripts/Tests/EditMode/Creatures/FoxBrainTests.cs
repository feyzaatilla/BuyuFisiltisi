using BuyuFisiltisi.Core.Creatures;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Creatures
{
    public class FoxBrainTests
    {
        [Test]
        public void SeesPlayer_StartsChasing()
        {
            var brain = new FoxBrain();

            brain.Tick(0.1f, true, 8f);

            Assert.AreEqual(FoxState.Chase, brain.State);
        }

        [Test]
        public void CatchesPlayer_StealsAndFlees()
        {
            var brain = new FoxBrain();
            brain.Tick(0.1f, true, 8f);

            var stole = brain.Tick(0.1f, true, 1f);

            Assert.IsTrue(stole);
            Assert.AreEqual(FoxState.Flee, brain.State);
        }

        [Test]
        public void LosesSight_ReturnsToPatrol()
        {
            var brain = new FoxBrain(loseSightSeconds: 2f);
            brain.Tick(0.1f, true, 8f);

            brain.Tick(1f, false, 8f);
            Assert.AreEqual(FoxState.Chase, brain.State);

            brain.Tick(1.1f, false, 8f);
            Assert.AreEqual(FoxState.Patrol, brain.State);
        }

        [Test]
        public void Flee_EndsAfterTime()
        {
            var brain = new FoxBrain(fleeSeconds: 3f);
            brain.Tick(0.1f, true, 8f);
            brain.Tick(0.1f, true, 1f);

            brain.Tick(3.1f, true, 1f);

            Assert.AreEqual(FoxState.Patrol, brain.State);
        }

        [Test]
        public void Stunned_CanBeCalmed_AndStaysCalm()
        {
            var brain = new FoxBrain();
            brain.Tick(0.1f, true, 8f);

            Assert.IsTrue(brain.Stun());
            Assert.IsTrue(brain.Calm());
            brain.Tick(10f, true, 0.5f);

            Assert.AreEqual(FoxState.Calm, brain.State);
            Assert.IsFalse(brain.Stun());
        }

        [Test]
        public void Calm_OnlyWorksWhenStunned()
        {
            var brain = new FoxBrain();

            Assert.IsFalse(brain.Calm());
            Assert.AreEqual(FoxState.Patrol, brain.State);
        }

        [Test]
        public void Stun_WearsOff()
        {
            var brain = new FoxBrain(stunSeconds: 2f);
            brain.Stun();

            brain.Tick(2.1f, true, 8f);

            Assert.AreEqual(FoxState.Patrol, brain.State);
        }
    }
}
