using BuyuFisiltisi.Core.Progress;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Progress
{
    public class QuestLogTests
    {
        private static QuestLog Create()
        {
            return new QuestLog(new[]
            {
                new QuestStep("a", "Birinci"),
                new QuestStep("b", "İkinci")
            });
        }

        [Test]
        public void NewLog_CurrentIsFirstStep()
        {
            var log = Create();

            Assert.AreEqual("a", log.Current.Value.Id);
            Assert.AreEqual(0, log.CompletedCount);
            Assert.IsFalse(log.IsFinished);
        }

        [Test]
        public void Complete_MovesToNextStep()
        {
            var log = Create();

            Assert.IsTrue(log.Complete("a"));

            Assert.AreEqual("b", log.Current.Value.Id);
            Assert.IsTrue(log.IsDone("a"));
        }

        [Test]
        public void Complete_LaterStepFirst_KeepsEarlierStepCurrent()
        {
            var log = Create();

            log.Complete("b");

            Assert.AreEqual("a", log.Current.Value.Id);
            Assert.AreEqual(1, log.CompletedCount);
        }

        [Test]
        public void Complete_SameOrUnknownStep_IsIgnored()
        {
            var log = Create();
            log.Complete("a");

            Assert.IsFalse(log.Complete("a"));
            Assert.IsFalse(log.Complete("yok"));
            Assert.IsFalse(log.Complete(null));
            Assert.AreEqual(1, log.CompletedCount);
        }

        [Test]
        public void AllStepsDone_IsFinishedAndHasNoCurrent()
        {
            var log = Create();
            log.Complete("a");
            log.Complete("b");

            Assert.IsTrue(log.IsFinished);
            Assert.IsFalse(log.Current.HasValue);
        }
    }
}
