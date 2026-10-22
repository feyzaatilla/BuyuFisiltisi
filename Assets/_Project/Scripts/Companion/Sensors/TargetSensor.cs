using System;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    public sealed class TargetSensor : ISensor<TargetReading>
    {
        private readonly Transform _self;
        private Transform _target;

        public TargetSensor(Transform self, float arriveDistance)
        {
            _self = self != null ? self : throw new ArgumentNullException(nameof(self));
            ArriveDistance = arriveDistance;
        }

        public float ArriveDistance { get; set; }

        public Transform Target => _target;

        public TargetReading Current { get; private set; } = TargetReading.None;

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void ClearTarget()
        {
            _target = null;
            Current = TargetReading.None;
        }

        public void Sense(float deltaTime)
        {
            Current = _target == null
                ? TargetReading.None
                : Evaluate(_self.position, _self.forward, _target.position, ArriveDistance);
        }

        public void Reset()
        {
            ClearTarget();
        }

        public static TargetReading Evaluate(Vector3 selfPosition, Vector3 selfForward, Vector3 targetPosition, float arriveDistance)
        {
            var toTarget = targetPosition - selfPosition;
            toTarget.y = 0f;
            var forward = selfForward;
            forward.y = 0f;

            var distance = toTarget.magnitude;
            var angle = distance > 0.0001f && forward.sqrMagnitude > 0.0001f
                ? Vector3.SignedAngle(forward, toTarget, Vector3.up)
                : 0f;

            return new TargetReading(true, distance, angle, distance <= arriveDistance);
        }
    }
}
