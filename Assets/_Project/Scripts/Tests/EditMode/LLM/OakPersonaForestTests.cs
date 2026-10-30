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
    }
}
