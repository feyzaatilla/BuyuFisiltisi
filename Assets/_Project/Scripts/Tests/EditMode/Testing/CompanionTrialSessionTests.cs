using System;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Testing;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Testing
{
    public class CompanionTrialSessionTests
    {
        private const string Task = "InteractWithTarget:fisil_test_hedefi";

        [Test]
        public void NewSession_IsEmpty()
        {
            var session = new CompanionTrialSession(10, Task);

            Assert.AreEqual(0, session.Attempts);
            Assert.AreEqual(0f, session.SuccessRate);
            Assert.IsFalse(session.IsComplete);
        }

        [Test]
        public void Constructor_RejectsZeroTrials()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompanionTrialSession(0, Task));
        }

        [Test]
        public void Record_CountsOutcomes()
        {
            var session = new CompanionTrialSession(4, Task);

            session.Record(Task, CompanionTaskOutcome.Succeeded, 6f);
            session.Record(Task, CompanionTaskOutcome.Failed, 2f);
            session.Record(Task, CompanionTaskOutcome.TimedOut, 20f);
            session.Record(Task, CompanionTaskOutcome.Cancelled, 1f);

            Assert.AreEqual(4, session.Attempts);
            Assert.AreEqual(1, session.Successes);
            Assert.AreEqual(1, session.Failures);
            Assert.AreEqual(1, session.TimeOuts);
            Assert.AreEqual(1, session.Cancels);
            Assert.IsTrue(session.IsComplete);
        }

        [Test]
        public void Record_IgnoresOtherTasks()
        {
            var session = new CompanionTrialSession(3, Task);

            var recorded = session.Record("InteractWithTarget:bridge_switch", CompanionTaskOutcome.Succeeded, 5f);

            Assert.IsFalse(recorded);
            Assert.AreEqual(0, session.Attempts);
        }

        [Test]
        public void Record_StopsWhenComplete()
        {
            var session = new CompanionTrialSession(1, Task);
            session.Record(Task, CompanionTaskOutcome.Succeeded, 5f);

            var recorded = session.Record(Task, CompanionTaskOutcome.Succeeded, 5f);

            Assert.IsFalse(recorded);
            Assert.AreEqual(1, session.Attempts);
        }

        [Test]
        public void AverageSeconds_UsesOnlySuccesses()
        {
            var session = new CompanionTrialSession(3, Task);
            session.Record(Task, CompanionTaskOutcome.Succeeded, 6f);
            session.Record(Task, CompanionTaskOutcome.Succeeded, 8f);
            session.Record(Task, CompanionTaskOutcome.TimedOut, 20f);

            Assert.AreEqual(7f, session.AverageSuccessSeconds, 0.001f);
            Assert.AreEqual(2f / 3f, session.SuccessRate, 0.001f);
        }

        [Test]
        public void RecordTimeout_CountsAsTimeOut()
        {
            var session = new CompanionTrialSession(2, Task);

            session.RecordTimeout(30f);

            Assert.AreEqual(1, session.TimeOuts);
            Assert.AreEqual(CompanionTaskOutcome.TimedOut, session.LastOutcome);
        }

        [Test]
        public void Summary_ShowsCountsRateAndAverage()
        {
            var session = new CompanionTrialSession(10, Task);
            for (var i = 0; i < 9; i++)
                session.Record(Task, CompanionTaskOutcome.Succeeded, 6.8f);
            session.Record(Task, CompanionTaskOutcome.Failed, 3f);

            Assert.AreEqual("10 denemede 9 başarılı (%90), ortalama süre 6.8 sn", session.Summary());
        }

        [Test]
        public void Record_WithStartName_CountsPerStart()
        {
            var session = new CompanionTrialSession(3, Task);
            session.Record(Task, CompanionTaskOutcome.Succeeded, 5f, "A");
            session.Record(Task, CompanionTaskOutcome.Failed, 5f, "A");
            session.Record(Task, CompanionTaskOutcome.Succeeded, 5f, "B");

            Assert.AreEqual(2, session.AttemptsFrom("A"));
            Assert.AreEqual(1, session.SuccessesFrom("A"));
            Assert.AreEqual(1, session.SuccessesFrom("B"));
            Assert.AreEqual(0, session.AttemptsFrom("C"));
        }

        [Test]
        public void StartBreakdown_KeepsFirstSeenOrder()
        {
            var session = new CompanionTrialSession(4, Task);
            session.Record(Task, CompanionTaskOutcome.Succeeded, 5f, "B");
            session.Record(Task, CompanionTaskOutcome.Succeeded, 5f, "A");
            session.RecordTimeout(30f, "B");

            Assert.AreEqual("B: 1/2, A: 1/1", session.StartBreakdown());
        }
    }
}
