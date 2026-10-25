using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;

namespace BuyuFisiltisi.Core.Actions
{
    public sealed class OpenDoorAction : IGameAction
    {
        private readonly string _doorId;

        public OpenDoorAction(string doorId)
        {
            _doorId = doorId;
        }

        public string Name => "Kapıyı aç";

        public void Execute(IEventBus bus)
        {
            bus.Publish(new DoorOpenRequestedEvent(_doorId));
        }
    }
}