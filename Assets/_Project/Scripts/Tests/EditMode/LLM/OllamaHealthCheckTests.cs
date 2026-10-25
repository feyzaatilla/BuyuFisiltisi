using System;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OllamaHealthCheckTests
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

        private static OllamaSettings Settings(float timeout = 10f) => new OllamaSettings("http://localhost:11434/", "gemma2:2b", timeout, 0.7f);

        private static OllamaHealth Check(FakeTransport transport, float timeout = 10f)
        {
            return new OllamaHealthCheck(transport, Settings(timeout)).CheckAsync().GetAwaiter().GetResult();
        }

        [Test]
        public void ModelFound_IsReady()
        {
            Assert.AreEqual(OllamaHealth.Ready, Check(new FakeTransport(() => HttpResult.Ok("{\"details\":{}}"))));
        }

        [Test]
        public void NotFound_IsModelMissing()
        {
            Assert.AreEqual(OllamaHealth.ModelMissing, Check(new FakeTransport(() => HttpResult.Fail(HttpFailure.HttpError, 404))));
        }

        [Test]
        public void OtherHttpError_IsUnreachable()
        {
            Assert.AreEqual(OllamaHealth.Unreachable, Check(new FakeTransport(() => HttpResult.Fail(HttpFailure.HttpError, 500))));
        }

        [Test]
        public void ConnectionFailed_IsUnreachable()
        {
            Assert.AreEqual(OllamaHealth.Unreachable, Check(new FakeTransport(() => HttpResult.Fail(HttpFailure.ConnectionFailed))));
        }

        [Test]
        public void Timeout_IsUnreachable()
        {
            Assert.AreEqual(OllamaHealth.Unreachable, Check(new FakeTransport(() => HttpResult.Fail(HttpFailure.Timeout))));
        }

        [Test]
        public void Cancelled_IsUnknown()
        {
            Assert.AreEqual(OllamaHealth.Unknown, Check(new FakeTransport(() => HttpResult.Fail(HttpFailure.Cancelled))));
        }

        [Test]
        public void TransportThrows_IsUnreachable()
        {
            Assert.AreEqual(OllamaHealth.Unreachable, Check(new FakeTransport(() => throw new InvalidOperationException())));
        }

        [Test]
        public void Request_AsksShowEndpointForConfiguredModel()
        {
            var transport = new FakeTransport(() => HttpResult.Ok("{}"));

            Check(transport);

            Assert.AreEqual("http://localhost:11434/api/show", transport.LastUrl);
            StringAssert.Contains("\"model\":\"gemma2:2b\"", transport.LastJson);
        }

        [Test]
        public void Timeout_IsCappedForQuickCheck()
        {
            var transport = new FakeTransport(() => HttpResult.Ok("{}"));

            Check(transport, 30f);

            Assert.AreEqual(5f, transport.LastTimeout);
        }
    }
}
