using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    [RequireComponent(typeof(CollectibleFlower))]
    public sealed class ElusiveFlower : MonoBehaviour
    {
        [SerializeField] private Transform[] _spots = new Transform[0];
        [SerializeField] private Transform _player;
        [SerializeField] private float _fleeDistance = 3.5f;
        [SerializeField] private int _escapes = 2;
        [SerializeField] private float _hideSeconds = 0.6f;

        private Renderer _renderer;
        private int _used;
        private float _visibleAt;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_player == null || _used >= _escapes || _spots.Length == 0)
                return;

            if (_renderer != null && !_renderer.enabled && Time.time >= _visibleAt)
                _renderer.enabled = true;

            var offset = transform.position - _player.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > _fleeDistance * _fleeDistance)
                return;

            _used++;
            transform.position = _spots[_used % _spots.Length].position;

            if (_renderer != null)
            {
                _renderer.enabled = false;
                _visibleAt = Time.time + _hideSeconds;
            }

            Debug.Log("[Yalancı Çiçek] Kayboldu! Kaçış: " + _used + "/" + _escapes);
        }
    }
}
