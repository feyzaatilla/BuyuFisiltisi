using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakDoorHintTests
    {
        [TestCase("kapıyı nasıl açabilirim")]
        [TestCase("Kapıyı açmak için ne yapmalıyım?")]
        [TestCase("MÜHÜRLÜ KAPI NASIL AÇILIR")]
        public void DoorQuestions_AreRecognized(string text)
        {
            Assert.IsTrue(OakDoorHint.IsDoorQuestion(text));
        }

        [TestCase("Yaşlı Meşe, izin verirsen kapıyı benim için açar mısın?")]
        [TestCase("Köprüyü nasıl geçerim?")]
        [TestCase("Merhaba bilge ağaç.")]
        public void OtherMessages_AreNotDoorQuestions(string text)
        {
            Assert.IsFalse(OakDoorHint.IsDoorQuestion(text));
        }

        [Test]
        public void Apply_LockedDoor_GivesTrustHint()
        {
            var result = OakDoorHint.Apply(new OakResponse("Fısıl'a şalteri çekmesini söyle.", OakAction.GiveHint, 1), false);

            Assert.AreEqual(OakDoorHint.LockedReply, result.Reply);
            Assert.AreEqual(OakAction.GiveHint, result.Action);
            Assert.AreEqual(0, result.TrustDelta);
        }

        [Test]
        public void Apply_ReadyDoor_TellsToAskPolitely()
        {
            var result = OakDoorHint.Apply(new OakResponse("Şalteri çek.", OakAction.OpenDoor, 0), true);

            Assert.AreEqual(OakDoorHint.ReadyReply, result.Reply);
            Assert.AreEqual(OakAction.GiveHint, result.Action);
        }
    
        [TestCase("iznin olursa kapıyı benim için açar mısın")]
        [TestCase("Yaşlı Meşe, izin verirsen kapıyı benim için açar mısın?")]
        [TestCase("Lütfen kapıyı aç")]
        [TestCase("Kapıyı açabilir misin?")]
        public void PoliteRequests_AreDoorRequests(string text)
        {
            Assert.IsTrue(OakDoorHint.IsDoorRequest(text));
        }

        [TestCase("Kapıyı nasıl açabilirim?")]
        [TestCase("Köprüyü açar mısın?")]
        [TestCase("Merhaba")]
        public void OtherMessages_AreNotDoorRequests(string text)
        {
            Assert.IsFalse(OakDoorHint.IsDoorRequest(text));
        }

        [Test]
        public void Request_Trusted_OpensDoor()
        {
            var result = OakDoorHint.Request(new OakResponse("Biraz daha zaman gerekiyor.", OakAction.None, 0), true);

            Assert.AreEqual(OakAction.OpenDoor, result.Action);
            Assert.AreEqual(OakDoorHint.OpenReply, result.Reply);
        }

        [Test]
        public void Request_TrustedWithMissingTask_NamesTask()
        {
            var missing = new[] { "Yaralı ateş böceği hâlâ ışıksız." };

            var result = OakDoorHint.Request(new OakResponse("Peki.", OakAction.OpenDoor, 0), true, missing);

            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual(OakDoorHint.UnfinishedReply + " Yaralı ateş böceği hâlâ ışıksız. " + OakDoorHint.UnfinishedEnding, result.Reply);
        }

        [Test]
        public void Request_NotTrusted_KeepsDoorClosed()
        {
            var result = OakDoorHint.Request(new OakResponse("Peki.", OakAction.OpenDoor, 0), false);

            Assert.AreEqual(OakAction.None, result.Action);
            Assert.AreEqual(OakDoorHint.NotYetReply, result.Reply);
        }
    }
}
