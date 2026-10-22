using System;

namespace BuyuFisiltisi.Companion.Sensors
{
    public sealed class StuckDetector : ISensor<bool>
    {
        private readonly Func<float> _speedSource;
        private readonly StuckDetectorSettings _settings;
        private float _slowTime;

        public StuckDetector(Func<float> speedSource, StuckDetectorSettings settings)
        {
            _speedSource = speedSource ?? throw new ArgumentNullException(nameof(speedSource));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool Current { get; private set; }

        public float SlowTime => _slowTime;

        public void Sense(float deltaTime)
        {
            if (_speedSource() <= _settings.SpeedThreshold)
                _slowTime += deltaTime;
            else
                _slowTime = 0f;

            Current = _slowTime >= _settings.TimeThreshold;
        }

        public void Reset()
        {
            _slowTime = 0f;
            Current = false;
        }
    }
}
