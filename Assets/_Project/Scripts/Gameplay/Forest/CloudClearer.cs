using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class CloudClearer : MonoBehaviour, IRequiresEventBus
    {
        public const string ClearedDeedId = "clouds_dispersed";

        [SerializeField] private DarkCloud[] _clouds = new DarkCloud[0];
        [SerializeField] private Transform _player;
        [SerializeField] private float _range = 7f;

        private IEventBus _eventBus;
        private int _dispersed;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var cast = (keyboard != null && keyboard.rKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (_player == null || !cast)
                return;

            var cloud = FindNearest();
            if (cloud == null)
            {
                Debug.Log("[Bulut] Yakında kara bulut yok.");
                return;
            }

            cloud.Disperse();
            _dispersed++;
            Debug.Log("[Bulut] Kara bulut dağıldı: " + _dispersed + "/" + _clouds.Length);
            _eventBus?.Publish(new CloudDispersedEvent(_dispersed, _clouds.Length));

            if (_dispersed >= _clouds.Length)
                _eventBus?.Publish(new DeedRecordedEvent(ClearedDeedId, "Ormanın üzerindeki kara bulutları dağıttı."));
        }

        private DarkCloud FindNearest()
        {
            DarkCloud best = null;
            var bestDistance = _range * _range;
            foreach (var cloud in _clouds)
            {
                if (cloud == null || cloud.IsDispersed)
                    continue;

                var offset = cloud.transform.position - _player.position;
                offset.y = 0f;
                var distance = offset.sqrMagnitude;
                if (distance > bestDistance)
                    continue;

                bestDistance = distance;
                best = cloud;
            }

            return best;
        }
    }
}
