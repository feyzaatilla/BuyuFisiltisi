using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OllamaChatEnvelopeTests
    {
        [Test]
        public void ChatResponse_ContentIsRead()
        {
            var body = "{\"model\":\"gemma2:2b\",\"message\":{\"role\":\"assistant\",\"content\":\"{\\\"reply\\\":\\\"Selam.\\\",\\\"action\\\":\\\"none\\\",\\\"trust_delta\\\":0}\"},\"done\":true}";

            var ok = OllamaChatEnvelope.TryReadContent(body, out var content);

            Assert.IsTrue(ok);
            Assert.IsTrue(OakResponseParser.TryParse(content, out var response));
            Assert.AreEqual("Selam.", response.Reply);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("<html>502 Bad Gateway</html>")]
        [TestCase("{\"error\":\"model 'gemma2:2b' not found\"}")]
        [TestCase("{\"message\":{\"role\":\"assistant\",\"content\":\"\"}}")]
        public void BadBody_Fails(string body)
        {
            Assert.IsFalse(OllamaChatEnvelope.TryReadContent(body, out var content));
            Assert.IsNull(content);
        }
    }
}
