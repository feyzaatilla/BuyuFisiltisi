using System;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakDialogueClientTests
    {
        private sealed class FakeTransport : IHttpTransport
        {
            private readonly Func<HttpResult> respond;

            public FakeTransport(Func<HttpResult> respond)
            {
                this.respond = respond;
            }

            public string LastUrl { get; private set; }
            public string LastJson { get; private set; }
            public float LastTimeout { get; private set; }
            public int Calls { get; private set; }

            public Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
            {
                Calls++;
                LastUrl = url;
                LastJson = json;
                LastTimeout = timeoutSeconds;
                return Task.FromResult(respond());
            }
        }

        private static readonly ChatMessage[] Conversation =
        {
            ChatMessage.FromSystem("Sen Yaşlı Meşe'sin."),
            ChatMessage.FromUser("Merhaba bilge ağaç.")
        };

        private static OllamaSettings Settings => new OllamaSettings("http://localhost:11434/", "gemma2:2b", 8f, 0.7f);

        private static string Envelope(string content)
        {
            return "{\"message\":{\"role\":\"assistant\",\"content\":\"" + content.Replace("\"", "\\\"") + "\"},\"done\":true}";
        }

        private static OakDialogueResult Ask(FakeTransport transport)
        {
            return new OakDialogueClient(transport, Settings).AskAsync(Conversation).GetAwaiter().GetResult();
        }

        [Test]
        public void ValidReply_ReturnsOk()
        {
            var transport = new FakeTransport(() => HttpResult.Ok(Envelope("{\"reply\":\"Hoş geldin.\",\"action\":\"give_hint\",\"trust_delta\":1}")));

            var result = Ask(transport);

            Assert.AreEqual(OakDialogueStatus.Ok, result.Status);
            Assert.AreEqual("Hoş geldin.", result.Response.Reply);
            Assert.AreEqual(OakAction.GiveHint, result.Response.Action);
            Assert.AreEqual(1, result.Response.TrustDelta);
        }

        [Test]
        public void Request_UsesChatUrlTimeoutAndJsonFormat()
        {
            var transport = new FakeTransport(() => HttpResult.Ok(Envelope("{\"reply\":\"Selam.\"}")));

            Ask(transport);

            Assert.AreEqual("http://localhost:11434/api/chat", transport.LastUrl);
            Assert.AreEqual(8f, transport.LastTimeout);
            StringAssert.Contains("\"model\":\"gemma2:2b\"", transport.LastJson);
            StringAssert.Contains("\"stream\":false", transport.LastJson);
            StringAssert.Contains("\"format\":\"json\"", transport.LastJson);
            StringAssert.Contains("\"role\":\"system\"", transport.LastJson);
            StringAssert.Contains("Merhaba bilge", transport.LastJson);
        }

        [TestCase(HttpFailure.ConnectionFailed, OakDialogueStatus.ServerUnavailable)]
        [TestCase(HttpFailure.HttpError, OakDialogueStatus.ServerUnavailable)]
        [TestCase(HttpFailure.Timeout, OakDialogueStatus.Timeout)]
        [TestCase(HttpFailure.Cancelled, OakDialogueStatus.Cancelled)]
        public void TransportFailure_MapsToStatusWithFallback(HttpFailure failure, OakDialogueStatus expected)
        {
            var result = Ask(new FakeTransport(() => HttpResult.Fail(failure)));

            Assert.AreEqual(expected, result.Status);
            Assert.AreEqual(OakResponse.FallbackReply, result.Response.Reply);
            Assert.AreEqual(OakAction.None, result.Response.Action);
        }

        [Test]
        public void TransportThrows_IsServerUnavailable()
        {
            var result = Ask(new FakeTransport(() => throw new InvalidOperationException("bağlantı yok")));

            Assert.AreEqual(OakDialogueStatus.ServerUnavailable, result.Status);
        }

        [Test]
        public void BrokenEnvelope_IsInvalidResponse()
        {
            var result = Ask(new FakeTransport(() => HttpResult.Ok("<html>502</html>")));

            Assert.AreEqual(OakDialogueStatus.InvalidResponse, result.Status);
            Assert.AreEqual(OakResponse.FallbackReply, result.Response.Reply);
        }

        [Test]
        public void ContentWithoutJson_IsInvalidResponse()
        {
            var result = Ask(new FakeTransport(() => HttpResult.Ok(Envelope("Ben bir ağacım."))));

            Assert.AreEqual(OakDialogueStatus.InvalidResponse, result.Status);
            Assert.AreEqual("Ben bir ağacım.", result.RawContent);
        }

        [Test]
        public void EmptyConversation_Throws()
        {
            var client = new OakDialogueClient(new FakeTransport(() => HttpResult.Ok("")), Settings);

            Assert.Throws<ArgumentException>(() => client.AskAsync(new ChatMessage[0]).GetAwaiter().GetResult());
        }
    }
}
