using BuyuFisiltisi.Core.Creatures;
using BuyuFisiltisi.Gameplay.Forest;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class FoxAnimationTests
    {
        private static readonly Color Orange = new Color(0.9f, 0.5f, 0.2f, 1f);

        [TestCase(FoxState.Patrol, FoxAnimation.Walk)]
        [TestCase(FoxState.Chase, FoxAnimation.Gallop)]
        [TestCase(FoxState.Flee, FoxAnimation.Gallop)]
        [TestCase(FoxState.Stunned, FoxAnimation.HitReact)]
        [TestCase(FoxState.Calm, FoxAnimation.Rest)]
        public void StateFor_PicksClipForEachState(FoxState state, string expected)
        {
            Assert.AreEqual(expected, FoxAnimation.StateFor(state));
        }

        [Test]
        public void ClipName_RemovesArmaturePrefix()
        {
            Assert.AreEqual("Walk", FoxAnimation.ClipName("AnimalArmature|Walk"));
            Assert.AreEqual("Idle_2", FoxAnimation.ClipName("Idle_2"));
            Assert.AreEqual(string.Empty, FoxAnimation.ClipName(null));
        }

        [Test]
        public void Loops_OnlyForRepeatingMoves()
        {
            Assert.IsTrue(FoxAnimation.Loops(FoxAnimation.Walk));
            Assert.IsTrue(FoxAnimation.Loops(FoxAnimation.Gallop));
            Assert.IsTrue(FoxAnimation.Loops(FoxAnimation.Rest));
            Assert.IsFalse(FoxAnimation.Loops(FoxAnimation.HitReact));
            Assert.IsFalse(FoxAnimation.Loops("Attack"));
        }

        [Test]
        public void Tint_ShadowFoxIsDarkAndPurple()
        {
            var color = FoxAnimation.Tint(Orange, 0f);

            Assert.AreEqual(Orange.r * FoxAnimation.ShadowTint.r, color.r, 0.001f);
            Assert.AreEqual(Orange.b * FoxAnimation.ShadowTint.b, color.b, 0.001f);
            Assert.Less(color.r, Orange.r);
            Assert.AreEqual(1f, color.a, 0.001f);
        }

        [Test]
        public void Tint_CalmFoxGetsOriginalColor()
        {
            var color = FoxAnimation.Tint(Orange, 1f);

            Assert.AreEqual(Orange.r, color.r, 0.001f);
            Assert.AreEqual(Orange.g, color.g, 0.001f);
            Assert.AreEqual(Orange.b, color.b, 0.001f);
        }
    }
}
