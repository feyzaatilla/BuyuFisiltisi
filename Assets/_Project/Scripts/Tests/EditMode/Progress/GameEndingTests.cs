using BuyuFisiltisi.Core.Progress;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Progress
{
    public class GameEndingTests
    {
        [Test]
        public void NewEnding_IsNotOver()
        {
            var ending = new EndingState();

            Assert.IsFalse(ending.IsOver);
            Assert.AreEqual(GameOutcome.None, ending.Outcome);
        }

        [Test]
        public void TryEnd_Good_SetsOutcomeAndTexts()
        {
            var ending = new EndingState();

            Assert.IsTrue(ending.TryEnd(GameOutcome.Good));
            Assert.IsTrue(ending.IsOver);
            Assert.IsNotEmpty(ending.Title);
            Assert.IsNotEmpty(ending.Story);
        }

        [Test]
        public void TryEnd_FirstOutcomeWins()
        {
            var ending = new EndingState();
            ending.TryEnd(GameOutcome.Bad);

            Assert.IsFalse(ending.TryEnd(GameOutcome.Good));
            Assert.AreEqual(GameOutcome.Bad, ending.Outcome);
        }

        [Test]
        public void TryEnd_None_IsIgnored()
        {
            var ending = new EndingState();

            Assert.IsFalse(ending.TryEnd(GameOutcome.None));
            Assert.IsFalse(ending.IsOver);
        }
    }
}
