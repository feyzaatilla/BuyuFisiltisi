
using BuyuFisiltisi.Core.Actions;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.LLM.Actions;
using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Actions
{
    public class GameActionFactoryTests
    {
        private readonly GameActionFactory _factory = new GameActionFactory();

        [Test]
        public void OpenDoor_PublishesDoorOpenRequest()
        {
            var bus = new EventBus();
            string doorId = null;
            bus.Subscribe<DoorOpenRequestedEvent>(e => doorId = e.DoorId);

            var action = _factory.Create(new OakResponse("Geç bakalım.", OakAction.OpenDoor, 1));
            action.Execute(bus);

            Assert.IsInstanceOf<OpenDoorAction>(action);
            Assert.AreEqual(GameActionFactory.SealedDoorId, doorId);
        }

        [Test]
        public void GiveHint_CarriesOakReply()
        {
            var bus = new EventBus();
            string hint = null;
            bus.Subscribe<OakHintRequestedEvent>(e => hint = e.Hint);

            _factory.Create(new OakResponse("Işık çayını dene.", OakAction.GiveHint, 0)).Execute(bus);

            Assert.AreEqual("Işık çayını dene.", hint);
        }

        [Test]
        public void Attack_PublishesDamage()
        {
            var bus = new EventBus();
            var damage = 0;
            bus.Subscribe<OakAttackedEvent>(e => damage = e.Damage);

            _factory.Create(new OakResponse("Defol!", OakAction.Attack, -2)).Execute(bus);

            Assert.AreEqual(GameActionFactory.AttackDamage, damage);
        }

        [Test]
        public void None_ReturnsNoAction()
        {
            var action = _factory.Create(new OakResponse("Merhaba.", OakAction.None, 0));

            Assert.AreSame(NoAction.Instance, action);
        }

        [Test]
        public void NullResponse_ReturnsNoAction()
        {
            Assert.AreSame(NoAction.Instance, _factory.Create(null));
        }
    }
}