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
        public void Prompt_WithEnoughTrust_HasOpenDoorExample()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(start: 3), null);

            StringAssert.Contains("kapıyı benim için açar mısın?", prompt);
            StringAssert.Contains("kapı açılıyor", prompt);
        }

        [Test]
        public void Prompt_WithoutEnoughTrust_HasNoOpenDoorExample()
        {
            var prompt = OakPersona.BuildSystemPrompt(new OakTrust(start: 0), null);

            StringAssert.Contains("kapıyı benim için açar mısın?", prompt);
            StringAssert.DoesNotContain("kapı açılıyor", prompt);
            StringAssert.Contains("Henüz sana yeterince güvenmiyorum", prompt);
        }
    }
}
