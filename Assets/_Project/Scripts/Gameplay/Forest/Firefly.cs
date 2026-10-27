using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class Firefly : MonoBehaviour, IInteractable, IRequiresEventBus
    {
        public const string HealedDeedId = "firefly_healed";

        [SerializeField] private string _interactionId = "firefly";
        [SerializeField] private string _displayName = "Ateş Böceği";
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private Transform _focusPoint;

        private IEventBus _eventBus;
        private bool _healed;

        public bool IsHealed => _healed;
        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_healed && _inventory != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            if (!_inventory.Inventory.TryConsume(Recipe.LightTeaId))
            {
                Debug.Log("[Ateş Böceği] Işık Çayı yok, iyileştiremedim.");
                return;
            }

            _healed = true;
            _eventBus?.Publish(new DeedRecordedEvent(HealedDeedId, "Yaralı ateş böceği Işık Çayı ile iyileştirildi."));
            Debug.Log("[Ateş Böceği] İyileşti.");
        }
    }
}