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

        public bool CanBrew(Recipe recipe)
        {
            if (recipe == null)
                return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (_inventory.Count(ingredient.ItemId) < ingredient.Amount)
                    return false;
            }

            return true;
        }

        public bool TryBrew(Recipe recipe)
        {
            if (!CanBrew(recipe))
                return false;

            foreach (var ingredient in recipe.Ingredients)
                _inventory.TryConsume(ingredient.ItemId, ingredient.Amount);

            _inventory.Add(recipe.ProductId);
            _eventBus.Publish(new RecipeCompletedEvent(recipe.Id));
            return true;
        }
    }
}
