using BuyuFisiltisi.UI.Menu;
using NUnit.Framework;
using UnityEngine;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class MenuBackdropLayoutTests
    {
        [Test]
        public void TreePositions_SameSeed_SameLayout()
        {
            var first = MenuBackdropLayout.TreePositions(11, 20, 7f, 20f);
            var second = MenuBackdropLayout.TreePositions(11, 20, 7f, 20f);

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void TreePositions_PlacesRequestedCount()
        {
            Assert.AreEqual(20, MenuBackdropLayout.TreePositions(11, 20, 7f, 20f).Count);
        }

        [Test]
        public void TreePositions_KeepViewToOakClear()
        {
            foreach (var position in MenuBackdropLayout.TreePositions(11, 20, 7f, 20f))
                Assert.IsFalse(MenuBackdropLayout.InCorridor(position), position.ToString());
        }

        [Test]
        public void TreePositions_DoNotOverlap()
        {
            var trees = MenuBackdropLayout.TreePositions(11, 20, 7f, 20f);
            for (var i = 0; i < trees.Count; i++)
                for (var j = i + 1; j < trees.Count; j++)
                    Assert.GreaterOrEqual(Vector3.Distance(trees[i], trees[j]), 2.2f);
        }

        [Test]
        public void CameraOffset_StartsAtOriginAndStaysSmall()
        {
            Assert.AreEqual(Vector3.zero, MenuBackdropLayout.CameraOffset(0f, 1.2f, 0.12f));

            for (var t = 0f; t < 200f; t += 7.3f)
                Assert.LessOrEqual(MenuBackdropLayout.CameraOffset(t, 1.2f, 0.12f).magnitude, 1.2f * 1.2f);
        }
    }
}
