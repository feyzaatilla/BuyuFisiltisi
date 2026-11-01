using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakPersonaForestTests
    {
        [Test]
        public void Prompt_ExplainsBridgeSwitchAndFisil()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("Orman bilgisi", prompt);
            StringAssert.Contains("şalter", prompt);
            StringAssert.Contains("Fısıl", prompt);
        }

        [Test]
        public void Prompt_HasBridgeHintExample()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("Köprüyü nasıl geçerim?", prompt);
        }

        [Test]
        public void Prompt_StillListsDeedsAfterForestKnowledge()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), new[] { "Ateş böceği iyileştirildi." });

            StringAssert.Contains("- Ateş böceği iyileştirildi.", prompt);
        }

        [Test]
        public void Prompt_UsesSpellBookInsteadOfOldKeys()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("Büyü Kitabı", prompt);
            StringAssert.DoesNotContain("B emri", prompt);
            StringAssert.DoesNotContain("F ile", prompt);
        }

        [Test]
        public void Prompt_KnowsNewForestPlaces()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("kazan", prompt);
            StringAssert.Contains("Sessizlik Merhemi", prompt);
            StringAssert.Contains("kara bulut", prompt);
            StringAssert.Contains("kök parkuru", prompt);
        }

        [Test]
        public void Prompt_TellsOakToNameKnownDeeds()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("adıyla an", prompt);
        }
    }
}
