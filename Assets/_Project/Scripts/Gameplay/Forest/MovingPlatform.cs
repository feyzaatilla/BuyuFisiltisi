using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 _travel = new Vector3(6f, 0f, 0f);
        [SerializeField] private float _speed = 0.5f;
        [SerializeField] private Transform _player;

        private Vector3 _start;
        private CharacterController _controller;
        private BoxCollider _box;

        private void Awake()
        {
            _start = transform.position;
            _box = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            var previous = transform.position;
            var t = (Mathf.Sin(Time.time * _speed * Mathf.PI) + 1f) * 0.5f;
            transform.position = _start - _travel * 0.5f + _travel * t;
            var delta = transform.position - previous;

            if (_player == null || _box == null)
                return;

            if (_controller == null)
                _controller = _player.GetComponentInParent<CharacterController>();
            if (_controller == null || !_controller.enabled)
                return;

            var bounds = _box.bounds;
            var feet = _controller.transform.position;
            var onTop = feet.x > bounds.min.x - 0.3f && feet.x < bounds.max.x + 0.3f &&
                        feet.z > bounds.min.z - 0.3f && feet.z < bounds.max.z + 0.3f &&
                        feet.y > bounds.max.y - 0.3f && feet.y < bounds.max.y + 0.5f;
            if (onTop)
                _controller.Move(delta);
        }
    }
}
