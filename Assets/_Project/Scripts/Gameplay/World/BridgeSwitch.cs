using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class BridgeSwitch : MonoBehaviour, IInteractable, IRequiresEventBus
    {
        public const string DefaultId = "bridge_switch";

        [SerializeField] private string _interactionId = DefaultId;
        [SerializeField] private string _displayName = "Köprü Şalteri";
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private Transform _focusPoint;
        [SerializeField] private Transform _handle;
        [SerializeField] private Vector3 _pulledHandleEuler = new Vector3(0f, 0f, -30f);

        private SwitchLatch _latch;
        private IEventBus _eventBus;

        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;
        public bool IsPulled => Latch.IsPulled;

        private SwitchLatch Latch => _latch ??= new SwitchLatch(_interactionId);

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public bool CanInteract(GameObject interactor)
        {
            return !Latch.IsPulled;
        }

        public void Interact(GameObject interactor)
        {
            if (!Latch.TryPull())
                return;

            if (_handle != null)
                _handle.localRotation = Quaternion.Euler(_pulledHandleEuler);

            if (_eventBus == null)
            {
                Debug.LogWarning("[Şalter] Olay yolu bağlanmamış, köprüye haber gidemedi.", this);
                return;
            }

            _eventBus.Publish(new SwitchPulledEvent(_interactionId));
        }
    }
}
