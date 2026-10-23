using BuyuFisiltisi.Companion.Movement;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode
{
    public class FollowPointTests
    {
        [Test]
        public void Calculate_ReturnsPointAtFollowDistance()
        {
            var goal = FollowPoint.Calculate(Vector3.zero, new Vector3(10f, 0f, 0f), 2f);

            Assert.AreEqual(2f, Vector3.Distance(Vector3.zero, goal), 0.001f);
        }

        [Test]
        public void Calculate_StaysOnFollowerSideOfLeader()
        {
            var goal = FollowPoint.Calculate(Vector3.zero, new Vector3(0f, 0f, -8f), 1.5f);

            Assert.Less(goal.z, 0f);
            Assert.AreEqual(0f, goal.x, 0.001f);
        }

        [Test]
        public void Calculate_IgnoresHeightDifference()
        {
            var goal = FollowPoint.Calculate(Vector3.zero, new Vector3(5f, 3f, 0f), 1f);

            Assert.AreEqual(0f, goal.y, 0.001f);
            Assert.AreEqual(1f, goal.x, 0.001f);
        }

        [Test]
        public void Calculate_WhenAlreadyClose_KeepsFollowerPosition()
        {
            var follower = new Vector3(0.5f, 0f, 0.5f);

            var goal = FollowPoint.Calculate(Vector3.zero, follower, 2f);

            Assert.AreEqual(follower, goal);
        }

        [Test]
        public void IsWithin_UsesHorizontalDistanceOnly()
        {
            Assert.IsTrue(FollowPoint.IsWithin(Vector3.zero, new Vector3(1f, 5f, 0f), 1.2f));
            Assert.IsFalse(FollowPoint.IsWithin(Vector3.zero, new Vector3(2f, 0f, 0f), 1.2f));
        }
    }
}
