using BuyuFisiltisi.Core.Events;

namespace BuyuFisiltisi.Core.Actions
{
    public sealed class NoAction : IGameAction
    {
        public static readonly NoAction Instance = new NoAction();

        private NoAction()
        {
        }

        public string Name => "Hiçbir şey";

        public void Execute(IEventBus bus)
        {
        }
    }
}