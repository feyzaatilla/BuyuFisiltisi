namespace BuyuFisiltisi.Core.GameEvents
{
    public readonly struct ItemCollectedEvent
    {
        public readonly string ItemId;
        public readonly int Amount;
        public readonly int Total;

        public ItemCollectedEvent(string itemId, int amount, int total)
        {
            ItemId = itemId;
            Amount = amount;
            Total = total;
        }
    }
}