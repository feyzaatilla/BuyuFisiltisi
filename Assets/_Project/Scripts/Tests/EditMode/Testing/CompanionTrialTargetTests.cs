using BuyuFisiltisi.Testing;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.Testing
{
    public class CompanionTrialTargetTests
    {
        private GameObject _object;
        private CompanionTrialTarget _target;

        [SetUp]
        public void SetUp()
        {
            _object = new GameObject("FisilTestHedefi");
            _target = _object.AddComponent<CompanionTrialTarget>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
        }

        [Test]
        public void TaskName_MatchesCompanionBrainFormat()
        {
            Assert.AreEqual("InteractWithTarget:fisil_test_hedefi", _target.TaskName);
        }

        [Test]
        public void Interact_MarksDone()
        {
            _target.Interact(null);

            Assert.IsTrue(_target.IsDone);
            Assert.IsFalse(_target.CanInteract(null));
            Assert.AreEqual(1, _target.InteractCount);
        }

        [Test]
        public void ResetForTrial_AllowsInteractAgain()
        {
            _target.Interact(null);
            _target.ResetForTrial();
            _target.Interact(null);

            Assert.IsFalse(_target.CanInteract(null));
            Assert.AreEqual(2, _target.InteractCount);
        }
    }
}
