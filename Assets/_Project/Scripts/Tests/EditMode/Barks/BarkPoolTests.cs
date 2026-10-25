using System.Collections.Generic;
using System.Linq;
using BuyuFisiltisi.Companion.Barks;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Barks
{
    public class BarkPoolTests
    {
        private static BarkPool CreatePool(float cooldown) =>
            new BarkPool(new Dictionary<BarkTrigger, string[]>
            {
                [BarkTrigger.Waiting] = new[] { "a", "b", "c" },
                [BarkTrigger.TaskFailed] = new[] { "tek" }
            }, cooldown, new System.Random(1));

        [Test]
        public void Bark_ComesFromTriggerLines()
        {
            var pool = CreatePool(0f);

            Assert.IsTrue(pool.TryGetBark(BarkTrigger.Waiting, 0f, out var text));
            CollectionAssert.Contains(new[] { "a", "b", "c" }, text);
        }

        [Test]
        public void Cooldown_BlocksSecondBark()
        {
            var pool = CreatePool(4f);

            Assert.IsTrue(pool.TryGetBark(BarkTrigger.Waiting, 0f, out _));
            Assert.IsFalse(pool.TryGetBark(BarkTrigger.Waiting, 2f, out _));
            Assert.IsTrue(pool.TryGetBark(BarkTrigger.Waiting, 4f, out _));
        }

        [Test]
        public void SameLine_IsNotRepeatedBackToBack()
        {
            var pool = CreatePool(0f);
            var lines = Enumerable.Range(0, 20).Select(i =>
            {
                pool.TryGetBark(BarkTrigger.Waiting, i, out var text);
                return text;
            }).ToList();

            for (var i = 1; i < lines.Count; i++)
                Assert.AreNotEqual(lines[i - 1], lines[i]);
        }

        [Test]
        public void MissingTrigger_ReturnsFalse()
        {
            var pool = CreatePool(0f);

            Assert.IsFalse(pool.TryGetBark(BarkTrigger.Following, 0f, out var text));
            Assert.IsNull(text);
        }

        [Test]
        public void SingleLine_IsAllowedToRepeat()
        {
            var pool = CreatePool(0f);

            Assert.IsTrue(pool.TryGetBark(BarkTrigger.TaskFailed, 0f, out var first));
            Assert.IsTrue(pool.TryGetBark(BarkTrigger.TaskFailed, 1f, out var second));
            Assert.AreEqual(first, second);
        }
    }
}
