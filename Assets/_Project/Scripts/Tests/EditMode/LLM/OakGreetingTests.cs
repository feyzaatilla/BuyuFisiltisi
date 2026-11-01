using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakGreetingTests
    {
        [Test]
        public void Build_WithoutDeeds_SaysNothingSeenYet()
        {
            var text = OakGreeting.Build(new string[0], new OakTrust());

            StringAssert.Contains("henüz görmedim", text);
        }

        [Test]
        public void Build_NullDeeds_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => OakGreeting.Build(null, null));
        }

        [Test]
        public void Build_ListsEveryKnownDeed()
        {
            var text = OakGreeting.Build(new[]
            {
                "Yaralı ateş böceği Işık Çayı ile iyileştirildi.",
                "Ormanın üzerindeki kara bulutları dağıttı."
            }, new OakTrust());

            StringAssert.Contains("• Yaralı ateş böceği Işık Çayı ile iyileştirildi.", text);
            StringAssert.Contains("• Ormanın üzerindeki kara bulutları dağıttı.", text);
            StringAssert.Contains("unutmadım", text);
        }

        [Test]
        public void Build_SkipsBlankDeeds()
        {
            var text = OakGreeting.Build(new[] { " ", "Kök iyileşti." }, new OakTrust());

            StringAssert.Contains("• Kök iyileşti.", text);
            Assert.AreEqual(1, text.Split('•').Length - 1);
        }

        [Test]
        public void Build_ShowsOnlyLatestDeeds()
        {
            var text = OakGreeting.Build(new[] { "1", "2", "3", "4", "5" }, new OakTrust());

            StringAssert.DoesNotContain("• 2", text);
            StringAssert.Contains("• 3", text);
            StringAssert.Contains("• 5", text);
        }

        [Test]
        public void Build_WithEnoughTrust_SaysTrusted()
        {
            var text = OakGreeting.Build(new[] { "Kök iyileşti." }, new OakTrust(start: 3));

            StringAssert.Contains("güveniyorum", text);
        }
    }
}
