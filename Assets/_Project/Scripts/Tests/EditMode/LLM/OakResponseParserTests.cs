using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakResponseParserTests
    {
        [Test]
        public void ValidJson_IsParsed()
        {
            var ok = OakResponseParser.TryParse("{\"reply\":\"Hoş geldin, cadı.\",\"action\":\"give_hint\",\"trust_delta\":1}", out var response);

            Assert.IsTrue(ok);
            Assert.AreEqual("Hoş geldin, cadı.", response.Reply);
            Assert.AreEqual(OakAction.GiveHint, response.Action);
            Assert.AreEqual(1, response.TrustDelta);
        }

        [Test]
        public void JsonInsideExtraText_IsParsed()
        {
            var ok = OakResponseParser.TryParse("```json\n{\"reply\":\"Kapı açılsın.\",\"action\":\"open_door\",\"trust_delta\":2}\n```", out var response);

            Assert.IsTrue(ok);
            Assert.AreEqual(OakAction.OpenDoor, response.Action);
        }

        [Test]
        public void TrustDeltaOutOfRange_IsClamped()
        {
            OakResponseParser.TryParse("{\"reply\":\"Asla!\",\"action\":\"attack\",\"trust_delta\":-9}", out var low);
            OakResponseParser.TryParse("{\"reply\":\"Dostum!\",\"action\":\"none\",\"trust_delta\":7}", out var high);

            Assert.AreEqual(OakResponse.MinTrustDelta, low.TrustDelta);
            Assert.AreEqual(OakResponse.MaxTrustDelta, high.TrustDelta);
        }

        [Test]
        public void UnknownAction_BecomesNone()
        {
            OakResponseParser.TryParse("{\"reply\":\"Hmm.\",\"action\":\"dance\",\"trust_delta\":0}", out var response);

            Assert.AreEqual(OakAction.None, response.Action);
        }

        [Test]
        public void MissingFields_UseSafeDefaults()
        {
            var ok = OakResponseParser.TryParse("{\"reply\":\"Sadece dinliyorum.\"}", out var response);

            Assert.IsTrue(ok);
            Assert.AreEqual(OakAction.None, response.Action);
            Assert.AreEqual(0, response.TrustDelta);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Merhaba, ben bir ağacım.")]
        [TestCase("{\"reply\":")]
        [TestCase("{\"action\":\"open_door\",\"trust_delta\":2}")]
        public void BrokenInput_FailsWithFallback(string raw)
        {
            var ok = OakResponseParser.TryParse(raw, out var response);

            Assert.IsFalse(ok);
            Assert.AreEqual(OakResponse.FallbackReply, response.Reply);
            Assert.AreEqual(OakAction.None, response.Action);
            Assert.AreEqual(0, response.TrustDelta);
        }

        [Test]
        public void ActionNames_AreCaseInsensitive()
        {
            Assert.AreEqual(OakAction.OpenDoor, OakResponseParser.ParseAction(" OPEN_DOOR "));
            Assert.AreEqual(OakAction.Attack, OakResponseParser.ParseAction("Attack"));
        }
    }
}
