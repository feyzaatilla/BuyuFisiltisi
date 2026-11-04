using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakToneGuardTests
    {
        [TestCase("Merhaba Yaşlı Meşe, nasılsın?")]
        [TestCase("Ormanın çok güzel.")]
        [TestCase("Kapıyı açar mısın lütfen?")]
        [TestCase("Bunu hemen açıklar mısın?")]
        [TestCase("")]
        public void Polite_IsNeutral(string text)
        {
            Assert.AreEqual(OakTone.Neutral, OakToneGuard.Judge(text));
        }

        [TestCase("Ormanını beğenmedim")]
        [TestCase("Sen aptal bir ağaçsın")]
        [TestCase("Kapıyı hemen aç!")]
        [TestCase("SUS ARTIK")]
        [TestCase("Defol git")]
        public void Rude_IsDetected(string text)
        {
            Assert.AreEqual(OakTone.Rude, OakToneGuard.Judge(text));
        }

        [TestCase("Seni yakıp kül edeceğim!")]
        [TestCase("kesicem seni kökünden")]
        [TestCase("Bu ormanı yok edeceğim")]
        [TestCase("SENİ YAKARIM")]
        public void Threat_IsDetected(string text)
        {
            Assert.AreEqual(OakTone.Threat, OakToneGuard.Judge(text));
        }

        [Test]
        public void Threat_WinsOverRude()
        {
            Assert.AreEqual(OakTone.Threat, OakToneGuard.Judge("Aptal ağaç, seni yakarım!"));
        }

        [Test]
        public void Apply_Rude_OverridesFriendlyModel()
        {
            var result = OakToneGuard.Apply(new OakResponse("Teşekkürler!", OakAction.OpenDoor, 2), OakTone.Rude);

            Assert.AreEqual(OakToneGuard.RudeDelta, result.TrustDelta);
            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual(OakToneGuard.RudeReply, result.Reply);
        }

        [Test]
        public void Apply_Threat_KeepsAttackAndHarsherModelDelta()
        {
            var result = OakToneGuard.Apply(new OakResponse("Git buradan!", OakAction.Attack, -2), OakTone.Threat);

            Assert.AreEqual(-2, result.TrustDelta);
            Assert.AreEqual(OakAction.Attack, result.Action);
        }

        [Test]
        public void Apply_Neutral_CapsRiseAtOne()
        {
            var result = OakToneGuard.Apply(new OakResponse("Çok naziksin.", OakAction.None, 2), OakTone.Neutral);

            Assert.AreEqual(OakToneGuard.MaxRise, result.TrustDelta);
            Assert.AreEqual("Çok naziksin.", result.Reply);
        }

        [Test]
        public void Apply_Neutral_KeepsNegativeDelta()
        {
            var result = OakToneGuard.Apply(new OakResponse("Hmm.", OakAction.None, -1), OakTone.Neutral);

            Assert.AreEqual(-1, result.TrustDelta);
        }

        [Test]
        public void Escalate_ThreatReachingThreshold_Attacks()
        {
            var result = OakToneGuard.Escalate(new OakResponse(OakToneGuard.ThreatReply, OakAction.None, -2), OakTone.Threat, -1, -3, -5);

            Assert.AreEqual(OakAction.Attack, result.Action);
            Assert.AreEqual(OakToneGuard.AttackReply, result.Reply);
            Assert.AreEqual(-2, result.TrustDelta);
        }

        [Test]
        public void Escalate_ThreatAboveThreshold_DoesNotAttack()
        {
            var result = OakToneGuard.Escalate(new OakResponse(OakToneGuard.ThreatReply, OakAction.None, -2), OakTone.Threat, 0, -3, -5);

            Assert.AreEqual(OakAction.None, result.Action);
        }

        [Test]
        public void Escalate_RudeAtThreshold_Attacks()
        {
            var result = OakToneGuard.Escalate(new OakResponse(OakToneGuard.RudeReply, OakAction.None, -1), OakTone.Rude, -2, -3, -5);

            Assert.AreEqual(OakAction.Attack, result.Action);
        }

        [Test]
        public void Escalate_NeutralAtThreshold_DoesNotAttack()
        {
            var result = OakToneGuard.Escalate(new OakResponse("Özür dilerim.", OakAction.None, 0), OakTone.Neutral, -3, -3, -5);

            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual("Özür dilerim.", result.Reply);
        }

        [Test]
        public void Escalate_ReachingMinimum_AlwaysAttacks()
        {
            var result = OakToneGuard.Escalate(new OakResponse("Hmm.", OakAction.None, -1), OakTone.Neutral, -4, -3, -5);

            Assert.AreEqual(OakAction.Attack, result.Action);
        }
    }
}
