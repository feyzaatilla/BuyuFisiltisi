namespace BuyuFisiltisi.Core.GameEvents
{
    /// <summary>Bir şalter çekildiğinde yayınlanır (Fısıl köprü şalterini çektiğinde).</summary>
    public readonly struct SwitchPulledEvent
    {
        public readonly string SwitchId;
        public SwitchPulledEvent(string switchId) { SwitchId = switchId; }
    }

    /// <summary>Köprü tamamen indiğinde ve NavMesh güncellendiğinde yayınlanır.</summary>
    public readonly struct BridgeLoweredEvent
    {
    }

    /// <summary>Mühürlü kapı açıldığında yayınlanır.</summary>
    public readonly struct DoorOpenedEvent
    {
        public readonly string DoorId;
        public DoorOpenedEvent(string doorId) { DoorId = doorId; }
    }
}
