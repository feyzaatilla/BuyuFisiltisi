using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakPersonaTests
    {
        [Test]
        public void Prompt_ContainsRulesAndAllActions()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("Yaşlı Meşe", prompt);
            StringAssert.Contains("\"reply\"", prompt);
            StringAssert.Contains("\"trust_delta\"", prompt);
            foreach (var name in new[] { "none", "open_door", "give_hint", "attack" })
                StringAssert.Contains(name, prompt);
        }

        [Test]
        public void Prompt_ShowsCurrentTrustAndThreshold()
        {
            var trust = new OakTrust(start: 2);

            var prompt = OakPersona.BuildSystemPrompt(trust, null);

            StringAssert.Contains("güvenin 2", prompt);
            StringAssert.Contains("en az 3", prompt);
        }

        [Test]
        public void Prompt_ListsKnownDeeds()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), new[] { "Ateş böceklerini iyileştirdi.", " ", "Işık Çayı'nı demledi." });

            StringAssert.Contains("- Ateş böceklerini iyileştirdi.", prompt);
            StringAssert.Contains("- Işık Çayı'nı demledi.", prompt);
            StringAssert.DoesNotContain("Henüz bilinen bir iyiliği yok.", prompt);
        }

        [Test]
        public void Prompt_WithoutDeeds_SaysNone()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), new string[0]);

            StringAssert.Contains("Henüz bilinen bir iyiliği yok.", prompt);
        }

        [Test]
        public void ActionName_MatchesParser()
        {
            foreach (OakAction action in System.Enum.GetValues(typeof(OakAction)))
                Assert.AreEqual(action, OakResponseParser.ParseAction(OakPersona.ActionName(action)));
        }
    }
}
