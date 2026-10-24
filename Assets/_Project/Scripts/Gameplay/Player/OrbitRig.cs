using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    public sealed class OrbitRig
    {
        private readonly float _minPitch;
        private readonly float _maxPitch;
        private readonly float _minDistance;
        private readonly float _maxDistance;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public float Distance { get; private set; }

        public OrbitRig(float yaw, float pitch, float distance, float minPitch, float maxPitch, float minDistance, float maxDistance)
        {
            _minPitch = minPitch;
            _maxPitch = maxPitch;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            Yaw = Mathf.Repeat(yaw, 360f);
            Pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            Distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        public Quaternion Rotation => Quaternion.Euler(Pitch, Yaw, 0f);

        public Vector3 Offset => Rotation * new Vector3(0f, 0f, -Distance);

        public void Rotate(float yawDelta, float pitchDelta)
        {
            Yaw = Mathf.Repeat(Yaw + yawDelta, 360f);
            Pitch = Mathf.Clamp(Pitch + pitchDelta, _minPitch, _maxPitch);
        }

        public void Zoom(float delta)
        {
            Distance = Mathf.Clamp(Distance - delta, _minDistance, _maxDistance);
        }
    }
}
