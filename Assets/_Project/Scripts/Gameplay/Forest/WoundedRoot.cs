using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Core.Navigation;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class WoundedRoot : MonoBehaviour, IRequiresEventBus
    {
        public const string HealedDeedId = "oak_root_healed";

        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Light _woundLight;
        [SerializeField] private float _radius = 2f;

        private IEventBus _eventBus;
        private bool _healed;
        private float _nextHint;

        public bool IsHealed => _healed;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = _radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_healed || other.gameObject.layer != GameLayers.Player || _inventory == null)
                return;

            if (!_inventory.Inventory.TryConsume(Recipe.SilenceSalveId))
            {
                if (Time.time >= _nextHint)
                {
                    _nextHint = Time.time + 5f;
                    Debug.Log("[Kök] Meşe'nin kökü yaralı. Sessizlik Merhemi lazım.");
                }

                return;
            }

            _healed = true;
            if (_woundLight != null)
                _woundLight.color = new Color(0.4f, 1f, 0.5f);

            _eventBus?.Publish(new DeedRecordedEvent(HealedDeedId, "Yaşlı Meşe'nin yaralı kökü Sessizlik Merhemi ile iyileştirildi."));
            Debug.Log("[Kök] Yaralı kök iyileşti.");
        }
    }
}
