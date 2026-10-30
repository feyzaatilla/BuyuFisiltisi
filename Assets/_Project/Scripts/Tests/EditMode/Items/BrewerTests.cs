using System;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Items
{
    public class BrewerTests
    {
        private const string Flower = "isik_cicegi";
        private const string Tea = "isik_cayi";

        [Test]
        public void TryBrew_WithEnoughFlowers_SpendsFlowersAndAddsTea()
        {
            var inventory = new Inventory();
            inventory.Add(Flower, 6);
            var brewer = new Brewer(inventory, new EventBus());

            var result = brewer.TryBrew(Recipe.LightTea);

            Assert.IsTrue(result);
            Assert.AreEqual(1, inventory.Count(Flower));
            Assert.AreEqual(1, inventory.Count(Tea));
        }

        [Test]
        public void TryBrew_WithoutEnoughFlowers_ChangesNothing()
        {
            var inventory = new Inventory();
            inventory.Add(Flower, 4);
            var brewer = new Brewer(inventory, new EventBus());

            var result = brewer.TryBrew(Recipe.LightTea);

            Assert.IsFalse(result);
            Assert.AreEqual(4, inventory.Count(Flower));
            Assert.AreEqual(0, inventory.Count(Tea));
        }

        [Test]
        public void TryBrew_PublishesRecipeCompleted_OnlyOnSuccess()
        {
            var inventory = new Inventory();
            var bus = new EventBus();
            var completed = new System.Collections.Generic.List<string>();
            bus.Subscribe<RecipeCompletedEvent>(e => completed.Add(e.RecipeId));
            var brewer = new Brewer(inventory, bus);

            brewer.TryBrew(Recipe.LightTea);
            inventory.Add(Flower, 5);
            brewer.TryBrew(Recipe.LightTea);

            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual(Recipe.LightTeaId, completed[0]);
        }

        [Test]
        public void TryBrew_NullRecipe_ReturnsFalse()
        {
            var brewer = new Brewer(new Inventory(), new EventBus());

            Assert.IsFalse(brewer.TryBrew(null));
        }

        [Test]
        public void Constructor_NullDependencies_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new Brewer(null, new EventBus()));
            Assert.Throws<ArgumentNullException>(() => new Brewer(new Inventory(), null));
        }

        [Test]
        public void Recipe_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentException>(() => new Recipe("", Flower, 3, Tea));
            Assert.Throws<ArgumentException>(() => new Recipe(Tea, "", 3, Tea));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Recipe(Tea, Flower, 0, Tea));
        }
    }
}