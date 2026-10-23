using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class BridgeLowering
    {
        private readonly float _raisedAngle;
        private readonly float _loweredAngle;
        private readonly float _duration;
        private float _elapsed;

        public BridgeLowering(float raisedAngle, float loweredAngle, float duration)
        {
            _raisedAngle = raisedAngle;
            _loweredAngle = loweredAngle;
            _duration = Mathf.Max(0.01f, duration);
        }

        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }
        public float Progress => Mathf.Clamp01(_elapsed / _duration);
        public float CurrentAngle => Mathf.Lerp(_raisedAngle, _loweredAngle, Mathf.SmoothStep(0f, 1f, Progress));

        public bool Begin()
        {
            if (IsRunning || IsFinished)
                return false;

            IsRunning = true;
            return true;
        }

        public bool Advance(float deltaTime)
        {
            if (!IsRunning)
                return false;

            _elapsed += Mathf.Max(0f, deltaTime);
            if (_elapsed < _duration)
                return false;

            IsRunning = false;
            IsFinished = true;
            return true;
        }
    }
}
