using System;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.Core.Actions;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakDialogueSessionDoorTests
    {
        private sealed class FakeTransport : IHttpTransport
        {
            private readonly Func<HttpResult> respond;

            public FakeTransport(Func<HttpResult> respond)
            {
                this.respond = respond;
            }

            public Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
            {
                return Task.FromResult(respond());
            }
        }

        private static OakDialogueSession Session(OakTrust trust, params string[] deeds)
        {
            var session = Create(trust);
            foreach (var deed in deeds)
                session.AddDeed(deed);
            return session;
        }

        private static OakDialogueSession Create(OakTrust trust)
        {
            var content = "{\"reply\":\"Büyü Kitabı'nı aç ve Fısıl'a şalteri çekmesini söyle.\",\"action\":\"give_hint\",\"trust_delta\":1}";
            var body = "{\"message\":{\"role\":\"assistant\",\"content\":\"" + content.Replace("\"", "\\\"") + "\"},\"done\":true}";
            var settings = new OllamaSettings("http://localhost:11434", "gemma2:2b", 8f, 0.7f);
            return new OakDialogueSession(new OakDialogueClient(new FakeTransport(() => HttpResult.Ok(body)), settings), trust);
        }

        [Test]
        public void DoorQuestion_LowTrust_GetsDoorHintNotSwitch()
        {
            var turn = Session(new OakTrust(start: 2)).TalkAsync("kapıyı nasıl açabilirim").GetAwaiter().GetResult();

            Assert.AreEqual(OakDoorHint.LockedReply, turn.Reply);
            Assert.AreEqual(2, turn.CurrentTrust);
        }

        [Test]
        public void DoorQuestion_HighTrust_TellsToAskPolitely()
        {
            var turn = Session(new OakTrust(start: 3), OakDeedCatalogTests.AllDeeds).TalkAsync("Kapıyı nasıl açabilirim?").GetAwaiter().GetResult();

            Assert.AreEqual(OakDoorHint.ReadyReply, turn.Reply);
            Assert.AreEqual(3, turn.CurrentTrust);
        }

        [Test]
        public void DoorQuestion_HighTrustMissingTask_NamesTask()
        {
            var turn = Session(new OakTrust(start: 3), "Ormanın üzerindeki kara bulutları dağıttı.").TalkAsync("Kapıyı nasıl açabilirim?").GetAwaiter().GetResult();

            StringAssert.StartsWith(OakDoorHint.UnfinishedReply, turn.Reply);
            StringAssert.Contains("ateş böceği", turn.Reply);
        }

        [Test]
        public void PoliteRequest_AtTrustThree_AlwaysOpensDoor()
        {
            var turn = Session(new OakTrust(start: 3), OakDeedCatalogTests.AllDeeds).TalkAsync("iznin olursa kapıyı benim için açar mısın").GetAwaiter().GetResult();

            Assert.IsInstanceOf<OpenDoorAction>(turn.Action);
            Assert.AreEqual(OakDoorHint.OpenReply, turn.Reply);
        }

        [Test]
        public void PoliteRequest_TrustThreeMissingFirefly_StaysClosed()
        {
            var deeds = new[]
            {
                OakDeedCatalogTests.AllDeeds[0],
                OakDeedCatalogTests.AllDeeds[2],
                OakDeedCatalogTests.AllDeeds[3],
                OakDeedCatalogTests.AllDeeds[4]
            };

            var turn = Session(new OakTrust(start: 3), deeds).TalkAsync("kapıyı açar mısın yaşlı meşe").GetAwaiter().GetResult();

            Assert.AreSame(NoAction.Instance, turn.Action);
            StringAssert.StartsWith(OakDoorHint.UnfinishedReply, turn.Reply);
            StringAssert.Contains("Işık Çayı", turn.Reply);
            StringAssert.EndsWith(OakDoorHint.UnfinishedEnding, turn.Reply);
            Assert.AreEqual(3, turn.CurrentTrust);
        }

        [Test]
        public void QuestionAfterAgac_IsNotCommand()
        {
            var turn = Session(new OakTrust(start: 3), OakDeedCatalogTests.AllDeeds).TalkAsync("dediklerini yaptım yaşlı ağaç kapıyı açar mısın").GetAwaiter().GetResult();

            Assert.IsInstanceOf<OpenDoorAction>(turn.Action);
            Assert.AreEqual(OakDoorHint.OpenReply, turn.Reply);
            Assert.AreEqual(3, turn.CurrentTrust);
        }

        [Test]
        public void PoliteRequest_LowTrust_StaysClosed()
        {
            var turn = Session(new OakTrust(start: 1)).TalkAsync("Kapıyı açabilir misin?").GetAwaiter().GetResult();

            Assert.AreSame(NoAction.Instance, turn.Action);
            Assert.AreEqual(OakDoorHint.NotYetReply, turn.Reply);
        }
    }
}
