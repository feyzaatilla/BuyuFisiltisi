using System;
using System.Collections.Generic;

namespace BuyuFisiltisi.Companion.Barks
{
    public sealed class BarkPool : ICompanionBarkProvider
    {
        private readonly IReadOnlyDictionary<BarkTrigger, string[]> _lines;
        private readonly Dictionary<BarkTrigger, int> _lastIndex = new Dictionary<BarkTrigger, int>();
        private readonly float _cooldown;
        private readonly Random _random;
        private float _nextAllowedTime = float.NegativeInfinity;

        public BarkPool(IReadOnlyDictionary<BarkTrigger, string[]> lines, float cooldown, Random random)
        {
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            _cooldown = Math.Max(0f, cooldown);
            _random = random ?? new Random();
        }

        public bool TryGetBark(BarkTrigger trigger, float time, out string text)
        {
            text = null;
            if (time < _nextAllowedTime)
                return false;

            if (!_lines.TryGetValue(trigger, out var options) || options == null || options.Length == 0)
                return false;

            var index = _random.Next(options.Length);
            if (options.Length > 1 && _lastIndex.TryGetValue(trigger, out var last) && index == last)
                index = (index + 1) % options.Length;

            _lastIndex[trigger] = index;
            _nextAllowedTime = time + _cooldown;
            text = options[index];
            return true;
        }
    }
}
