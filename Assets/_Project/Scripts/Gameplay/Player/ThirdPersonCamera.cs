using BuyuFisiltisi.Core.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _targetHeight = 1.5f;
        [SerializeField] private float _startYaw;
        [SerializeField] private float _startPitch = 25f;
        [SerializeField] private float _startDistance = 7f;
        [SerializeField] private float _minPitch = -10f;
        [SerializeField] private float _maxPitch = 70f;
        [SerializeField] private float _minDistance = 2f;
        [SerializeField] private float _maxDistance = 14f;
        [SerializeField] private float _lookSensitivity = 0.2f;
        [SerializeField] private float _zoomStep = 1f;
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private float _followSpeed = 2.5f;
        [SerializeField] private float _shoulderOffset = 0.5f;
        [SerializeField] private LayerMask _collisionMask = (1 << GameLayers.Ground) | (1 << GameLayers.Obstacle);

        private OrbitRig _rig;
        private IPlayerInput _input;
        private Vector3 _lastTargetPosition;
        private bool _hasLastPosition;

        private void Awake()
        {
            EnsureReady();
        }

        private void EnsureReady()
        {
            _rig ??= new OrbitRig(_startYaw, _startPitch, _startDistance, _minPitch, _maxPitch, _minDistance, _maxDistance);
            _input ??= new KeyboardMousePlayerInput();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetInput(IPlayerInput input)
        {
            _input = input;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            EnsureReady();

            if (_input.IsLooking)
            {
                var look = _input.Look;
                _rig.Rotate(look.x * _lookSensitivity, -look.y * _lookSensitivity);
            }

            else
                FollowBehind();

            var zoom = _input.Zoom;
            if (Mathf.Abs(zoom) > 0.01f)
                _rig.Zoom(Mathf.Sign(zoom) * _zoomStep);

            var pivot = _target.position + Vector3.up * _targetHeight + _rig.Rotation * Vector3.right * _shoulderOffset;
            var offset = _rig.Offset;
            var distance = offset.magnitude;
            var direction = offset / distance;
            if (Physics.SphereCast(pivot, _collisionRadius, direction, out var hit, distance, _collisionMask, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(hit.distance, 0.3f);

            transform.SetPositionAndRotation(pivot + direction * distance, _rig.Rotation);
        }

        private void FollowBehind()
        {
            var position = _target.position;
            if (!_hasLastPosition)
            {
                _lastTargetPosition = position;
                _hasLastPosition = true;
                return;
            }

            var move = position - _lastTargetPosition;
            _lastTargetPosition = position;
            move.y = 0f;
            if (move.sqrMagnitude < 0.0001f)
                return;

            var wanted = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            var delta = Mathf.DeltaAngle(_rig.Yaw, wanted);
            if (Mathf.Abs(delta) > 135f)
                return;

            _rig.Rotate(delta * Mathf.Clamp01(_followSpeed * Time.deltaTime), 0f);
        }
    }
}
