using System;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;

namespace BuyuFisiltisi.Core.Items
{
    public sealed class Brewer
    {
        private readonly Inventory _inventory;
        private readonly IEventBus _eventBus;

        public Brewer(Inventory inventory, IEventBus eventBus)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public bool TryBrew(Recipe recipe)
        {
            if (recipe == null || !_inventory.TryConsume(recipe.IngredientId, recipe.IngredientAmount))
                return false;

            _inventory.Add(recipe.ProductId);
            _eventBus.Publish(new RecipeCompletedEvent(recipe.Id));
            return true;
        }
    }
}