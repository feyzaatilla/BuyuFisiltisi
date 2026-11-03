using BuyuFisiltisi.Gameplay.Forest;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class OakMoodTests
    {
        private static readonly Color Original = new Color(0.2f, 0.5f, 0.2f);

        [Test]
        public void Mood_FollowsTrustAndClamps()
        {
            Assert.AreEqual(0f, OakMood.Mood(0), 0.001f);
            Assert.AreEqual(1f, OakMood.Mood(5), 0.001f);
            Assert.AreEqual(-1f, OakMood.Mood(-5), 0.001f);
            Assert.AreEqual(0.6f, OakMood.Mood(3), 0.001f);
            Assert.AreEqual(1f, OakMood.Mood(9), 0.001f);
            Assert.AreEqual(-1f, OakMood.Mood(-9), 0.001f);
        }

        [Test]
        public void NeutralTrust_KeepsOriginalLeaves()
        {
            Assert.AreEqual(Original, OakMood.Leaves(Original, 0));
        }

        [Test]
        public void FullTrust_MakesLeavesBloom()
        {
            AssertClose(OakMood.BloomingLeaves, OakMood.Leaves(Original, 5));
        }

        [Test]
        public void NoTrust_WithersLeaves()
        {
            AssertClose(OakMood.WitheredLeaves, OakMood.Leaves(Original, -5));
        }

        [Test]
        public void HigherTrust_IsGreener()
        {
            Assert.Greater(OakMood.Leaves(Original, 4).g, OakMood.Leaves(Original, 2).g);
        }

        [Test]
        public void Light_GetsWarmerWithTrustAndColderWithout()
        {
            var original = new Color(1f, 0.8f, 0.45f);

            AssertClose(OakMood.WarmLight, OakMood.Light(original, 5));
            AssertClose(OakMood.ColdLight, OakMood.Light(original, -5));
            Assert.Greater(OakMood.Light(original, -3).b, original.b);
        }

        private static void AssertClose(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 0.001f);
            Assert.AreEqual(expected.g, actual.g, 0.001f);
            Assert.AreEqual(expected.b, actual.b, 0.001f);
        }

        [Test]
        public void TintTargetsOakVisual()
        {
            Assert.AreEqual("YasliMese_Gorunum", OakMoodTint.TargetName);
        }
    }
}
