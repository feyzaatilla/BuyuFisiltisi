using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakResponseGateTests
    {
        [Test]
        public void OpenDoor_WithLowTrust_BecomesNone()
        {
            var trust = new OakTrust();

            var result = OakResponseGate.ApplyTurn(new OakResponse("Geç bakalım.", OakAction.OpenDoor, 1), trust);

            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual("Geç bakalım.", result.Reply);
            Assert.AreEqual(1, trust.Current);
        }

        [Test]
        public void OpenDoor_WhenDeltaReachesThreshold_IsKept()
        {
            var trust = new OakTrust(start: 1);

            var result = OakResponseGate.ApplyTurn(new OakResponse("Kapı senin için açık.", OakAction.OpenDoor, 2), trust);

            Assert.AreEqual(OakAction.OpenDoor, result.Action);
            Assert.AreEqual(3, trust.Current);
        }

        [Test]
        public void Attack_WithoutLowTrust_BecomesNone()
        {
            var trust = new OakTrust();

            var result = OakResponseGate.ApplyTurn(new OakResponse("Git!", OakAction.Attack, -2), trust);

            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual(-2, trust.Current);
        }

        [Test]
        public void Attack_WithLowTrust_IsKept()
        {
            var trust = new OakTrust(start: -2);

            var result = OakResponseGate.ApplyTurn(new OakResponse("Defol!", OakAction.Attack, -2), trust);

            Assert.AreEqual(OakAction.Attack, result.Action);
        }

        [Test]
        public void HintAndNone_PassThrough()
        {
            var trust = new OakTrust();
            var hint = new OakResponse("Vadiye bak.", OakAction.GiveHint, 0);

            Assert.AreSame(hint, OakResponseGate.ApplyTurn(hint, trust));
        }

        [Test]
        public void NullResponse_ReturnsFallback_AndKeepsTrust()
        {
            var trust = new OakTrust(start: 2);

            var result = OakResponseGate.ApplyTurn(null, trust);

            Assert.AreEqual(OakResponse.FallbackReply, result.Reply);
            Assert.AreEqual(2, trust.Current);
        }
    }
}
