using BuyuFisiltisi.Core.Interaction;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    public sealed class CollectibleFlower : MonoBehaviour, IInteractable
    {
        public const string LightFlowerId = "isik_cicegi";

        [SerializeField] private string _interactionId = "isik_cicegi_1";
        [SerializeField] private string _displayName = "Işık Çiçeği";
        [SerializeField] private string _itemId = LightFlowerId;
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private Transform _focusPoint;

        private bool _collected;

        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;

        public bool CanInteract(GameObject interactor)
        {
            return !_collected && _inventory != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            _collected = true;
            _inventory.Collect(_itemId);
            gameObject.SetActive(false);
        }
    }
}
