using System.Collections.Generic;

namespace BuyuFisiltisi.Core.Items
{
    public sealed class Inventory
    {
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

        public int Count(string itemId)
        {
            return _counts.TryGetValue(itemId, out var count) ? count : 0;
        }

        public void Add(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
                return;

            _counts[itemId] = Count(itemId) + amount;
        }

        public bool TryConsume(string itemId, int amount = 1)
        {
            if (amount <= 0 || Count(itemId) < amount)
                return false;

            _counts[itemId] = Count(itemId) - amount;
            return true;
        }
    }
}