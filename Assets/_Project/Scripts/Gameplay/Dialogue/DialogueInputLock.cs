using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Dialogue
{
    public sealed class DialogueInputLock : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private Behaviour[] _lockedBehaviours;

        private IEventBus _bus;

        public void InjectEventBus(IEventBus eventBus)
        {
            _bus?.Unsubscribe<DialogueFocusChangedEvent>(OnFocusChanged);
            _bus = eventBus;
            _bus?.Subscribe<DialogueFocusChangedEvent>(OnFocusChanged);
        }

        private void OnDestroy()
        {
            _bus?.Unsubscribe<DialogueFocusChangedEvent>(OnFocusChanged);
        }

        private void OnFocusChanged(DialogueFocusChangedEvent change)
        {
            if (_lockedBehaviours == null)
                return;

            foreach (var behaviour in _lockedBehaviours)
            {
                if (behaviour != null)
                    behaviour.enabled = !change.IsActive;
            }
        }
    }
}
