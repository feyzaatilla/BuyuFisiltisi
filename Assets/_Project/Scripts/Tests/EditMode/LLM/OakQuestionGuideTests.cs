using System.Collections.Generic;
using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakQuestionGuideTests
    {
        private static readonly List<string> NoDeeds = new List<string>();

        private static string Answer(string text, IReadOnlyCollection<string> deeds = null, bool canOpenDoor = false)
        {
            return OakQuestionGuide.TryAnswer(text, deeds ?? NoDeeds, canOpenDoor, out var reply) ? reply : null;
        }

        [Test]
        public void DoorQuestion_GetsDoorAnswer()
        {
            Assert.AreEqual(OakDoorHint.LockedReply, Answer("Kapıyı nasıl açabilirim?"));
            Assert.AreEqual(OakDoorHint.ReadyReply, Answer("Kapıyı nasıl açabilirim?", null, true));
        }

        [TestCase("Köprüyü nasıl geçerim?", OakQuestionGuide.BridgeReply)]
        [TestCase("Ateş böceğine nasıl yardım edebilirim?", OakQuestionGuide.FireflyReply)]
        [TestCase("Kökünü nasıl iyileştiririm?", OakQuestionGuide.RootReply)]
        [TestCase("Bulutları nasıl dağıtırım?", OakQuestionGuide.CloudReply)]
        [TestCase("Tilkiye ne yapmalıyım?", OakQuestionGuide.FoxReply)]
        [TestCase("Mağara nerede?", OakQuestionGuide.CaveReply)]
        [TestCase("İksir nasıl yapılır?", OakQuestionGuide.CauldronReply)]
        [TestCase("Güveni nasıl arttırabilirim?", OakQuestionGuide.TrustReply)]
        public void TopicQuestion_GetsRightAnswer(string text, string expected)
        {
            Assert.AreEqual(expected, Answer(text));
        }

        [Test]
        public void TaskQuestion_RemindsFirstMissingDeed()
        {
            Assert.AreEqual(OakDeedCatalog.MissingHints(NoDeeds)[0], Answer("Ne yapmalıyım?"));
        }

        [TestCase("ormanın için nasıl iyilik yapabilirim")]
        [TestCase("Ormana nasıl yardım edebilirim?")]
        [TestCase("Ne yapabilirim?")]
        public void HelpQuestions_RemindFirstMissingDeed(string text)
        {
            Assert.AreEqual(OakDeedCatalog.MissingHints(NoDeeds)[0], Answer(text));
        }

        [Test]
        public void TaskQuestion_AllDone_SaysSo()
        {
            var deeds = new List<string> { "bulut", "ateş böceği", "kök", "tilki", "mağara" };

            Assert.AreEqual(OakQuestionGuide.AllDoneReply, Answer("Bana bir ipucu ver", deeds));
        }

        [TestCase("Yaşlı Meşe, izin verirsen kapıyı benim için açar mısın?")]
        [TestCase("Hava nasıl?")]
        [TestCase("Kaç yaşındasın?")]
        [TestCase("Güven kazanmak istiyorum")]
        [TestCase("Ateş böceğini iyileştirdim")]
        public void OtherMessages_AreLeftToModel(string text)
        {
            Assert.IsNull(Answer(text));
        }
    }
}
