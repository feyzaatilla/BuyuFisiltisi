using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakReplyTrimTests
    {
        [Test]
        public void ShortReply_StaysTheSame()
        {
            Assert.AreEqual("Merhaba küçük cadı. Ne istiyorsun?", OakReplyTrim.Shorten("Merhaba küçük cadı. Ne istiyorsun?"));
        }

        [Test]
        public void LongReply_KeepsFirstTwoSentences()
        {
            var reply = "İyilik ormana yayılır.  Kökümü Sessizlik Merhemi ile iyileştir. Bu iyilik bir şaheser. Şimdi söyle.";

            Assert.AreEqual("İyilik ormana yayılır. Kökümü Sessizlik Merhemi ile iyileştir.", OakReplyTrim.Shorten(reply));
        }

        [Test]
        public void DotsInsideWords_DoNotSplit()
        {
            Assert.AreEqual("Saat 3.5 oldu. Git.", OakReplyTrim.Shorten("Saat 3.5 oldu. Git. Hemen."));
        }

        [Test]
        public void VeryLongSentence_IsCutAtWord()
        {
            var reply = new string('a', 10) + " " + string.Join(" ", System.Linq.Enumerable.Repeat("orman", 60));

            var result = OakReplyTrim.Shorten(reply);

            Assert.LessOrEqual(result.Length, OakReplyTrim.MaxLength + 1);
            Assert.IsTrue(result.EndsWith("…"));
        }

        [Test]
        public void Apply_KeepsActionAndDelta()
        {
            var response = new OakResponse("Bir. İki. Üç.", OakAction.GiveHint, 1);

            var result = OakReplyTrim.Apply(response);

            Assert.AreEqual("Bir. İki.", result.Reply);
            Assert.AreEqual(OakAction.GiveHint, result.Action);
            Assert.AreEqual(1, result.TrustDelta);
        }
    
        [Test]
        public void MarkdownStars_AreRemoved()
        {
            Assert.AreEqual("Yaşlı Meşe'nin kökü: Merhem sür.", OakReplyTrim.Shorten("**Yaşlı Meşe'nin kökü:** Merhem sür."));
        }
    }
}
