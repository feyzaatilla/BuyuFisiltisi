
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;

namespace BuyuFisiltisi.Core.Actions
{
    public sealed class GiveHintAction : IGameAction
    {
        private readonly string _hint;

        public GiveHintAction(string hint)
        {
            _hint = hint;
        }

        public string Name => "İpucu ver";

        public void Execute(IEventBus bus)
        {
            bus.Publish(new OakHintRequestedEvent(_hint));
        }
    }
}