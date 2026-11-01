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
        [SerializeField] private Transform _player;
        [SerializeField] private float _touchRadius = 1.8f;
        [SerializeField] private Light _glow;

        private IEventBus _eventBus;
        private bool _healed;
        private bool _touching;

        public bool IsHealed => _healed;
        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            if (_player == null || _healed)
                return;

            var offset = _player.position - transform.position;
            offset.y = 0f;
            var touching = offset.magnitude <= _touchRadius;
            if (touching && !_touching)
            {
                if (_inventory != null && _inventory.Inventory.Count(Recipe.LightTeaId) > 0)
                    Interact(_player.gameObject);
                else
                    GameMessage.Show("Ateş böceği yaralı, ışığı sönmek üzere. Ona kazanda demlenmiş bir Işık Çayı getir.", 3.5f);
            }

            _touching = touching;
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
            if (_glow != null)
            {
                _glow.intensity *= 2.5f;
                _glow.range *= 1.6f;
            }
            GameMessage.Show("Işık Çayını ateş böceğine içirdin. Kanatları yeniden ışıl ışıl parlıyor!", new Color(1f, 0.9f, 0.45f), 3.5f);
            _eventBus?.Publish(new DeedRecordedEvent(HealedDeedId, "Yaralı ateş böceği Işık Çayı ile iyileştirildi."));
            Debug.Log("[Ateş Böceği] İyileşti.");
        }
    }
}