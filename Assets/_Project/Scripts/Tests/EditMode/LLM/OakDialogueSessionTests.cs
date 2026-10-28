using System;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.Core.Actions;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakDialogueSessionTests
    {
        private sealed class FakeTransport : IHttpTransport
        {
            private readonly Func<HttpResult> respond;

            public FakeTransport(Func<HttpResult> respond)
            {
                this.respond = respond;
            }

            public string LastJson { get; private set; }
            public int Calls { get; private set; }

            public Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
            {
                Calls++;
                LastJson = json;
                return Task.FromResult(respond());
            }
        }

        private static OllamaSettings Settings => new OllamaSettings("http://localhost:11434", "gemma2:2b", 8f, 0.7f);

        private static string Envelope(string content)
        {
            return "{\"message\":{\"role\":\"assistant\",\"content\":\"" + content.Replace("\"", "\\\"") + "\"},\"done\":true}";
        }

        private static string Reply(string action, int delta)
        {
            return Envelope("{\"reply\":\"Merhaba.\",\"action\":\"" + action + "\",\"trust_delta\":" + delta + "}");
        }

        private static OakDialogueSession Session(FakeTransport transport, OakTrust trust = null)
        {
            return new OakDialogueSession(new OakDialogueClient(transport, Settings), trust);
        }

        private static OakTurn Talk(OakDialogueSession session, string text = "Selam")
        {
            return session.TalkAsync(text).GetAwaiter().GetResult();
        }

        [Test]
        public void PlainReply_ReturnsReplyWithoutAction()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("none", 0))));

            var turn = Talk(session);

            Assert.IsTrue(turn.IsOk);
            Assert.AreEqual("Merhaba.", turn.Reply);
            Assert.AreSame(NoAction.Instance, turn.Action);
            Assert.IsFalse(turn.TrustChanged);
        }

        [Test]
        public void TrustDelta_UpdatesTrustAndReportsChange()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("none", 1))));

            var turn = Talk(session);

            Assert.AreEqual(0, turn.PreviousTrust);
            Assert.AreEqual(1, turn.CurrentTrust);
            Assert.IsTrue(turn.TrustChanged);
            Assert.AreEqual(1, session.Trust.Current);
        }

        [Test]
        public void OpenDoor_WithoutEnoughTrust_BecomesNoAction()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("open_door", 2))));

            var turn = Talk(session);

            Assert.AreSame(NoAction.Instance, turn.Action);
        }

        [Test]
        public void OpenDoor_WithEnoughTrust_KeepsAction()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("open_door", 1))), new OakTrust(2));

            var turn = Talk(session);

            Assert.AreEqual(3, turn.CurrentTrust);
            Assert.IsInstanceOf<OpenDoorAction>(turn.Action);
        }

        [Test]
        public void ServerFailure_KeepsTrustAndHistory()
        {
            var session = Session(new FakeTransport(() => HttpResult.Fail(HttpFailure.ConnectionFailed)));

            var turn = Talk(session);

            Assert.IsFalse(turn.IsOk);
            Assert.AreEqual(OakDialogueStatus.ServerUnavailable, turn.Status);
            Assert.AreSame(NoAction.Instance, turn.Action);
            Assert.AreEqual(0, session.Trust.Current);
            Assert.AreEqual(0, session.HistoryCount);
        }

        [Test]
        public void Success_RecordsExchangeInHistory()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("none", 0))));

            Talk(session);

            Assert.AreEqual(2, session.HistoryCount);
        }

        [Test]
        public void Deeds_AreSentInSystemPrompt()
        {
            var transport = new FakeTransport(() => HttpResult.Ok(Reply("none", 0)));
            var session = Session(transport);
            session.AddDeed("firefly healed");

            Talk(session);

            StringAssert.Contains("firefly healed", transport.LastJson);
        }

        [Test]
        public void AddDeed_IgnoresEmptyAndDuplicateDeeds()
        {
            var session = Session(new FakeTransport(() => HttpResult.Ok(Reply("none", 0))));

            Assert.IsTrue(session.AddDeed("firefly healed"));
            Assert.IsFalse(session.AddDeed(" firefly healed "));
            Assert.IsFalse(session.AddDeed("   "));
            Assert.AreEqual(1, session.Deeds.Count);
        }

        [Test]
        public void EmptyText_Throws()
        {
            var transport = new FakeTransport(() => HttpResult.Ok(Reply("none", 0)));
            var session = Session(transport);

            Assert.Throws<ArgumentException>(() => Talk(session, "   "));
            Assert.AreEqual(0, transport.Calls);
        }
    }
}
