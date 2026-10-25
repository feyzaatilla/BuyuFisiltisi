using BuyuFisiltisi.Core.Events;

namespace BuyuFisiltisi.Core.Actions
{
    public interface IGameAction
    {
        string Name { get; }

        void Execute(IEventBus bus);
    }
}