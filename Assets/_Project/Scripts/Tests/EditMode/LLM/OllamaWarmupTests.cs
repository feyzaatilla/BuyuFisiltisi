using System;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OllamaWarmupTests
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

            public Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
            {
                LastUrl = url;
                LastJson = json;
                LastTimeout = timeoutSeconds;
                return Task.FromResult(respond());
            }
        }

        private static OllamaSettings Settings => new OllamaSettings("http://localhost:11434", "gemma2:2b", 10f, 0.7f);

        [Test]
        public void BuildRequest_ContainsModelAndKeepAlive()
        {
            var json = OllamaWarmup.BuildRequest(Settings);

            StringAssert.Contains("gemma2:2b", json);
            StringAssert.Contains(OllamaWarmup.KeepAlive, json);
        }

        [Test]
        public void Run_PostsToChatUrlWithLongTimeout()
        {
            var transport = new FakeTransport(() => HttpResult.Ok("{}"));

            var warm = OllamaWarmup.RunAsync(transport, Settings).GetAwaiter().GetResult();

            Assert.IsTrue(warm);
            Assert.AreEqual("http://localhost:11434/api/chat", transport.LastUrl);
            Assert.GreaterOrEqual(transport.LastTimeout, OllamaWarmup.MinTimeoutSeconds);
        }

        [Test]
        public void Run_ReturnsFalseWhenServerUnavailable()
        {
            var transport = new FakeTransport(() => HttpResult.Fail(HttpFailure.ConnectionFailed));

            Assert.IsFalse(OllamaWarmup.RunAsync(transport, Settings).GetAwaiter().GetResult());
        }

        [Test]
        public void Run_ReturnsFalseWhenTransportThrows()
        {
            var transport = new FakeTransport(() => throw new InvalidOperationException());

            Assert.IsFalse(OllamaWarmup.RunAsync(transport, Settings).GetAwaiter().GetResult());
        }
    }
}
