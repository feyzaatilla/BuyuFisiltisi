using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ThornTrap : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private RootParkour _parkour;
        [SerializeField] private Transform _spikes;
        [SerializeField] private float _hitRadius = 1.2f;
        [SerializeField] private float _hitDepth = 0.6f;
        [SerializeField] private float _upSeconds = 1.2f;
        [SerializeField] private float _downSeconds = 2f;
        [SerializeField] private float _offset;

        private float _timer;
        private Vector3 _lowered;
        private Vector3 _raised;

        private void Awake()
        {
            _timer = _offset;
            if (_spikes == null)
                return;

            _raised = _spikes.localPosition;
            _lowered = _raised + Vector3.down * 1.25f;
            _spikes.localPosition = _lowered;
        }

        private void Update()
        {
            if (_spikes == null || _player == null)
                return;

            _timer = (_timer + Time.deltaTime) % (_upSeconds + _downSeconds);
            var rising = _timer >= _downSeconds - 0.3f;
            var up = _timer >= _downSeconds;
            _spikes.localPosition = Vector3.MoveTowards(_spikes.localPosition, rising ? _raised : _lowered, Time.deltaTime * 7f);

            var offset = _player.position - transform.position;
            if (up && Mathf.Abs(offset.y) < 1.2f && Mathf.Abs(offset.x) <= _hitRadius && Mathf.Abs(offset.z) <= _hitDepth && _parkour != null)
                _parkour.Fail("Ah! Yerden dikenler fırladı. Dikenler inince geç!");
        }
    }
}
