using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    [RequireComponent(typeof(Animator))]
    public sealed class SpeedAnimator : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        [SerializeField] private Transform _mover;
        [SerializeField] private float _damping = 0.1f;

        private Animator _animator;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_mover == null)
                _mover = transform.parent != null ? transform.parent : transform;
            _lastPosition = _mover.position;
        }

        private void Update()
        {
            if (Time.deltaTime <= 0f)
                return;

            var delta = _mover.position - _lastPosition;
            _lastPosition = _mover.position;
            delta.y = 0f;
            _animator.SetFloat(SpeedId, delta.magnitude / Time.deltaTime, _damping, Time.deltaTime);
        }
    }
}
