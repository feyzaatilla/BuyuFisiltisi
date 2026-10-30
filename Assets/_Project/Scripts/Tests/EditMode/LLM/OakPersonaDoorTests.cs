using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakPersonaDoorTests
    {
        [Test]
        public void Prompt_WithEnoughTrust_TellsOakToOpenDoor()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(start: 3), null);

            StringAssert.Contains("yeterince güveniyorsun", prompt);
        }

        [Test]
        public void Prompt_WithoutEnoughTrust_TellsOakToRefuse()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(start: 1), null);

            StringAssert.Contains("henüz yetmiyor", prompt);
            StringAssert.DoesNotContain("yeterince güveniyorsun", prompt);
        }

        [Test]
        public void Prompt_HasPoliteDoorRequestExample()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(), null);

            StringAssert.Contains("kapıyı benim için açar mısın?", prompt);
        }
    }
}
