using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;

namespace BuyuFisiltisi.Core.Actions
{
    public sealed class AttackAction : IGameAction
    {
        private readonly int _damage;

        public AttackAction(int damage)
        {
            _damage = damage;
        }

        public string Name => "Saldır";

        public void Execute(IEventBus bus)
        {
            bus.Publish(new OakAttackedEvent(_damage));
        }
    }
}