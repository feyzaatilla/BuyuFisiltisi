using System;
using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Items
{
    public class BrewerTests
    {
        [Test]
        public void TryBrew_LightTea_SpendsBothFlowersAndAddsTea()
        {
            var inventory = new Inventory();
            inventory.Add(ItemCatalog.SilverHyacinth, 2);
            inventory.Add(ItemCatalog.NightViolet);
            var brewer = new Brewer(inventory, new EventBus());

            var result = brewer.TryBrew(Recipe.LightTea);

            Assert.IsTrue(result);
            Assert.AreEqual(1, inventory.Count(ItemCatalog.SilverHyacinth));
            Assert.AreEqual(0, inventory.Count(ItemCatalog.NightViolet));
            Assert.AreEqual(1, inventory.Count(Recipe.LightTeaId));
        }

        [Test]
        public void TryBrew_MissingOneIngredient_ChangesNothing()
        {
            var inventory = new Inventory();
            inventory.Add(ItemCatalog.RedWhistleWeed);
            var brewer = new Brewer(inventory, new EventBus());

            var result = brewer.TryBrew(Recipe.SilenceSalve);

            Assert.IsFalse(result);
            Assert.AreEqual(1, inventory.Count(ItemCatalog.RedWhistleWeed));
            Assert.AreEqual(0, inventory.Count(Recipe.SilenceSalveId));
        }

        [Test]
        public void TryBrew_AllThreeRecipes_WithSixFlowers()
        {
            var inventory = new Inventory();
            inventory.Add(ItemCatalog.SilverHyacinth, 2);
            inventory.Add(ItemCatalog.NightViolet);
            inventory.Add(ItemCatalog.RedWhistleWeed);
            inventory.Add(ItemCatalog.MistFlower);
            inventory.Add(ItemCatalog.RootPomegranate);
            var brewer = new Brewer(inventory, new EventBus());

            foreach (var recipe in Recipe.All)
                Assert.IsTrue(brewer.TryBrew(recipe), recipe.Id);

            Assert.AreEqual(1, inventory.Count(Recipe.LightTeaId));
            Assert.AreEqual(1, inventory.Count(Recipe.SilenceSalveId));
            Assert.AreEqual(1, inventory.Count(Recipe.WhisperEssenceId));
        }

        [Test]
        public void TryBrew_PublishesRecipeCompleted_OnlyOnSuccess()
        {
            var inventory = new Inventory();
            var bus = new EventBus();
            var completed = new List<string>();
            bus.Subscribe<RecipeCompletedEvent>(e => completed.Add(e.RecipeId));
            var brewer = new Brewer(inventory, bus);

            brewer.TryBrew(Recipe.WhisperEssence);
            inventory.Add(ItemCatalog.RootPomegranate);
            inventory.Add(ItemCatalog.SilverHyacinth);
            brewer.TryBrew(Recipe.WhisperEssence);

            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual(Recipe.WhisperEssenceId, completed[0]);
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
            var flower = new Ingredient(ItemCatalog.SilverHyacinth);
            Assert.Throws<ArgumentException>(() => new Recipe("", "urun", flower));
            Assert.Throws<ArgumentException>(() => new Recipe("tarif", "", flower));
            Assert.Throws<ArgumentException>(() => new Recipe("tarif", "urun"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ingredient(ItemCatalog.SilverHyacinth, 0));
            Assert.Throws<ArgumentException>(() => new Ingredient(""));
        }

        [Test]
        public void ItemCatalog_KnowsFlowersAndNames()
        {
            Assert.IsTrue(ItemCatalog.IsFlower(ItemCatalog.MistFlower));
            Assert.IsFalse(ItemCatalog.IsFlower(Recipe.LightTeaId));
            Assert.AreEqual("Sessizlik Merhemi", ItemCatalog.GetName(Recipe.SilenceSalveId));
        }
    }
}
