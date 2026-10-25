namespace BuyuFisiltisi.Core.GameEvents
{
    public readonly struct DoorOpenRequestedEvent
    {
        public readonly string DoorId;
        public DoorOpenRequestedEvent(string doorId) { DoorId = doorId; }
    }

    public readonly struct OakHintRequestedEvent
    {
        public readonly string Hint;
        public OakHintRequestedEvent(string hint) { Hint = hint; }
    }

    public readonly struct OakAttackedEvent
    {
        public readonly int Damage;
        public OakAttackedEvent(int damage) { Damage = damage; }
    }
}