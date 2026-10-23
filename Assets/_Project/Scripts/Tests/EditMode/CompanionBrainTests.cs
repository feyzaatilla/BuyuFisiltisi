using System.Collections.Generic;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode
{
    public class CompanionBrainTests
    {
        private sealed class FakeBody : ICompanionBody
        {
            public GameObject Self { get; set; }
            public bool HasArrived { get; set; }
            public bool IsStuck { get; set; }
            public bool CanMove { get; set; } = true;
            public int FollowCalls { get; private set; }
            public int StopCalls { get; private set; }
            public Vector3? LastDestination { get; private set; }

            public void FollowPlayer() => FollowCalls++;

            public bool MoveTo(Vector3 destination)
            {
                LastDestination = destination;
                return CanMove;
            }

            public void Stop() => StopCalls++;
        }

        private sealed class FakeTarget : IInteractable
        {
            private readonly Transform point;

            public FakeTarget(Transform point)
            {
                this.point = point;
            }

            public bool Available { get; set; } = true;
            public int InteractCalls { get; private set; }
            public GameObject LastInteractor { get; private set; }

            public string InteractionId => "test_switch";
            public string DisplayName => "Test Şalteri";
            public Transform InteractionPoint => point;
            public Transform FocusPoint => point;

            public bool CanInteract(GameObject interactor) => Available;

            public void Interact(GameObject interactor)
            {
                InteractCalls++;
                LastInteractor = interactor;
                Available = false;
            }
        }

        private GameObject self;
        private GameObject point;
        private FakeBody body;
        private FakeTarget target;
        private CompanionBrain brain;
        private List<CompanionTaskOutcome> outcomes;

        [SetUp]
        public void SetUp()
        {
            self = new GameObject("Fisil_Test");
            point = new GameObject("Salter_Nokta");
            point.transform.position = new Vector3(4f, 0f, 2f);

            body = new FakeBody { Self = self };
            target = new FakeTarget(point.transform);
            brain = new CompanionBrain(body, 10f);
            outcomes = new List<CompanionTaskOutcome>();
            brain.TaskFinished += (name, outcome, duration) => outcomes.Add(outcome);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(self);
            Object.DestroyImmediate(point);
        }

        [Test]
        public void StartsInWaiting()
        {
            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
            Assert.IsFalse(brain.HasTask);
        }

        [Test]
        public void FollowCommand_FollowsPlayer()
        {
            brain.Issue(CompanionCommand.FollowPlayer());

            Assert.AreEqual(CompanionStateId.Following, brain.State);
            Assert.AreEqual(1, body.FollowCalls);
        }

        [Test]
        public void WaitCommand_StopsBody()
        {
            brain.Issue(CompanionCommand.FollowPlayer());
            brain.Issue(CompanionCommand.Wait());

            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
            Assert.AreEqual(1, body.StopCalls);
        }

        [Test]
        public void InteractCommand_MovesToInteractionPoint()
        {
            brain.Issue(CompanionCommand.InteractWith(target));

            Assert.AreEqual(CompanionStateId.GoingToTarget, brain.State);
            Assert.AreEqual(point.transform.position, body.LastDestination);
            Assert.IsTrue(brain.HasTask);
        }

        [Test]
        public void ArrivingAtTarget_InteractsAndSucceeds()
        {
            brain.Issue(CompanionCommand.InteractWith(target));
            body.HasArrived = true;
            brain.Tick(0.5f);

            Assert.AreEqual(1, target.InteractCalls);
            Assert.AreSame(self, target.LastInteractor);
            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.Succeeded }, outcomes);
            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
        }

        [Test]
        public void FetchItem_ReturnsToPlayerAfterSuccess()
        {
            brain.Issue(CompanionCommand.FetchItem(target));
            body.HasArrived = true;
            brain.Tick(0.5f);

            Assert.AreEqual(CompanionStateId.Following, brain.State);
            Assert.AreEqual(1, body.FollowCalls);
        }

        [Test]
        public void NotArrivedYet_KeepsGoing()
        {
            brain.Issue(CompanionCommand.InteractWith(target));
            brain.Tick(1f);

            Assert.AreEqual(CompanionStateId.GoingToTarget, brain.State);
            Assert.AreEqual(0, target.InteractCalls);
            Assert.IsEmpty(outcomes);
        }

        [Test]
        public void StuckOnTheWay_FailsAndWaits()
        {
            brain.Issue(CompanionCommand.InteractWith(target));
            body.IsStuck = true;
            brain.Tick(0.5f);

            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.Failed }, outcomes);
            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
            Assert.AreEqual(0, target.InteractCalls);
        }

        [Test]
        public void TooSlow_TimesOut()
        {
            brain.Issue(CompanionCommand.InteractWith(target));
            brain.Tick(6f);
            brain.Tick(6f);

            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.TimedOut }, outcomes);
            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
        }

        [Test]
        public void UnreachableTarget_FailsImmediately()
        {
            body.CanMove = false;
            brain.Issue(CompanionCommand.InteractWith(target));

            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.Failed }, outcomes);
            Assert.AreEqual(CompanionStateId.Waiting, brain.State);
        }

        [Test]
        public void UsedTarget_FailsWithoutMoving()
        {
            target.Available = false;
            brain.Issue(CompanionCommand.InteractWith(target));

            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.Failed }, outcomes);
            Assert.IsNull(body.LastDestination);
        }

        [Test]
        public void NewCommandDuringTask_CancelsTask()
        {
            brain.Issue(CompanionCommand.InteractWith(target));
            brain.Issue(CompanionCommand.FollowPlayer());

            CollectionAssert.AreEqual(new[] { CompanionTaskOutcome.Cancelled }, outcomes);
            Assert.AreEqual(CompanionStateId.Following, brain.State);
            Assert.IsFalse(brain.HasTask);
        }

        [Test]
        public void StateChanged_ReportsPreviousAndCurrent()
        {
            var changes = new List<string>();
            brain.StateChanged += (previous, current) => changes.Add($"{previous}>{current}");

            brain.Issue(CompanionCommand.InteractWith(target));
            body.HasArrived = true;
            brain.Tick(0.1f);

            CollectionAssert.AreEqual(new[]
            {
                "Waiting>GoingToTarget",
                "GoingToTarget>Interacting",
                "Interacting>Waiting"
            }, changes);
        }
    }
}
