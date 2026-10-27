using BuyuFisiltisi.Core.Items;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Items
{
    public class InventoryTests
    {
        private const string Flower = "isik_cicegi";

        [Test]
        public void NewInventory_HasNothing()
        {
            var inventory = new Inventory();

            Assert.AreEqual(0, inventory.Count(Flower));
        }

        [Test]
        public void Add_IncreasesCount()
        {
            var inventory = new Inventory();

            inventory.Add(Flower);
            inventory.Add(Flower, 2);

            Assert.AreEqual(3, inventory.Count(Flower));
        }

        [Test]
        public void Add_IgnoresInvalidInput()
        {
            var inventory = new Inventory();

            inventory.Add(Flower, 0);
            inventory.Add(Flower, -5);
            inventory.Add("", 3);
            inventory.Add(null, 3);

            Assert.AreEqual(0, inventory.Count(Flower));
        }

        [Test]
        public void TryConsume_WithEnough_RemovesAndReturnsTrue()
        {
            var inventory = new Inventory();
            inventory.Add(Flower, 3);

            var result = inventory.TryConsume(Flower, 2);

            Assert.IsTrue(result);
            Assert.AreEqual(1, inventory.Count(Flower));
        }

        [Test]
        public void TryConsume_WithoutEnough_ChangesNothing()
        {
            var inventory = new Inventory();
            inventory.Add(Flower, 1);

            var result = inventory.TryConsume(Flower, 2);

            Assert.IsFalse(result);
            Assert.AreEqual(1, inventory.Count(Flower));
        }
    }
}