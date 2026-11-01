using System;

namespace BuyuFisiltisi.Core.Items
{
    public readonly struct Ingredient
    {
        public readonly string ItemId;
        public readonly int Amount;

        public Ingredient(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentException("Malzeme kimliği boş olamaz.", nameof(itemId));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Malzeme miktarı en az 1 olmalı.");

            ItemId = itemId;
            Amount = amount;
        }
    }
}
