using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    public sealed class PlayerInventory : MonoBehaviour, IRequiresEventBus
    {
        private readonly Inventory _inventory = new Inventory();
        private IEventBus _eventBus;

        public Inventory Inventory => _inventory;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Collect(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
                return;

            _inventory.Add(itemId, amount);
            _eventBus?.Publish(new ItemCollectedEvent(itemId, amount, _inventory.Count(itemId)));
        }
    }
}