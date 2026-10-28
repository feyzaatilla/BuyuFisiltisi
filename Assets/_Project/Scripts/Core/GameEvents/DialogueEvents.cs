namespace BuyuFisiltisi.Core.GameEvents
{
    public readonly struct DialogueFocusChangedEvent
    {
        public readonly bool IsActive;
        public DialogueFocusChangedEvent(bool isActive) { IsActive = isActive; }
    }
}
