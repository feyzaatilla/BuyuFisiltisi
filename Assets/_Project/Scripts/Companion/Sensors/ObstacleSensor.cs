using System;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    public sealed class ObstacleSensor : ISensor<ObstacleReading>
    {
        private readonly Transform _self;
        private readonly IRaycaster _raycaster;
        private readonly ObstacleSensorSettings _settings;

        public ObstacleSensor(Transform self, IRaycaster raycaster, ObstacleSensorSettings settings)
        {
            _self = self != null ? self : throw new ArgumentNullException(nameof(self));
            _raycaster = raycaster ?? throw new ArgumentNullException(nameof(raycaster));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public ObstacleReading Current { get; private set; } = ObstacleReading.Clear;

        public Vector3 Origin => _self.position + Vector3.up * _settings.Height;

        public Vector3 LeftDirection => Quaternion.AngleAxis(-_settings.SideAngle, Vector3.up) * Forward;

        public Vector3 CenterDirection => Forward;

        public Vector3 RightDirection => Quaternion.AngleAxis(_settings.SideAngle, Vector3.up) * Forward;

        public float Range => _settings.Range;

        private Vector3 Forward
        {
            get
            {
                var forward = _self.forward;
                forward.y = 0f;
                return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            }
        }

        public void Sense(float deltaTime)
        {
            var origin = Origin;
            var left = _raycaster.Cast(origin, LeftDirection, _settings.Range, out var leftDistance);
            var center = _raycaster.Cast(origin, CenterDirection, _settings.Range, out var centerDistance);
            var right = _raycaster.Cast(origin, RightDirection, _settings.Range, out var rightDistance);

            var nearest = Mathf.Min(left ? leftDistance : float.PositiveInfinity,
                Mathf.Min(center ? centerDistance : float.PositiveInfinity, right ? rightDistance : float.PositiveInfinity));

            Current = new ObstacleReading(left, center, right, nearest);
        }

        public void Reset()
        {
            Current = ObstacleReading.Clear;
        }
    }
}
