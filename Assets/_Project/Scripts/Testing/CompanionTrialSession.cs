using System;
using System.Collections.Generic;
using System.Globalization;
using BuyuFisiltisi.Core.GameEvents;

namespace BuyuFisiltisi.Testing
{
    public sealed class CompanionTrialSession
    {
        private readonly List<string> _startOrder = new List<string>();
        private readonly Dictionary<string, int> _startAttempts = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _startSuccesses = new Dictionary<string, int>();
        private float _successSeconds;

        public CompanionTrialSession(int trialCount, string taskName)
        {
            if (trialCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(trialCount));

            TrialCount = trialCount;
            TaskName = taskName ?? throw new ArgumentNullException(nameof(taskName));
        }

        public int TrialCount { get; }
        public string TaskName { get; }
        public int Attempts { get; private set; }
        public int Successes { get; private set; }
        public int Failures { get; private set; }
        public int TimeOuts { get; private set; }
        public int Cancels { get; private set; }
        public CompanionTaskOutcome? LastOutcome { get; private set; }
        public float LastSeconds { get; private set; }

        public bool IsComplete => Attempts >= TrialCount;
        public float SuccessRate => Attempts == 0 ? 0f : (float)Successes / Attempts;
        public float AverageSuccessSeconds => Successes == 0 ? 0f : _successSeconds / Successes;

        public bool Record(string taskName, CompanionTaskOutcome outcome, float seconds)
        {
            return Record(taskName, outcome, seconds, null);
        }

        public bool Record(string taskName, CompanionTaskOutcome outcome, float seconds, string startName)
        {
            if (IsComplete || !string.Equals(taskName, TaskName, StringComparison.Ordinal))
                return false;

            if (!string.IsNullOrEmpty(startName))
                CountStart(startName, outcome == CompanionTaskOutcome.Succeeded);

            Attempts++;
            LastOutcome = outcome;
            LastSeconds = seconds;

            switch (outcome)
            {
                case CompanionTaskOutcome.Succeeded:
                    Successes++;
                    _successSeconds += seconds;
                    break;
                case CompanionTaskOutcome.TimedOut:
                    TimeOuts++;
                    break;
                case CompanionTaskOutcome.Cancelled:
                    Cancels++;
                    break;
                default:
                    Failures++;
                    break;
            }

            return true;
        }

        public bool RecordTimeout(float seconds)
        {
            return RecordTimeout(seconds, null);
        }

        public bool RecordTimeout(float seconds, string startName)
        {
            return Record(TaskName, CompanionTaskOutcome.TimedOut, seconds, startName);
        }

        public int AttemptsFrom(string startName)
        {
            return startName != null && _startAttempts.TryGetValue(startName, out var count) ? count : 0;
        }

        public int SuccessesFrom(string startName)
        {
            return startName != null && _startSuccesses.TryGetValue(startName, out var count) ? count : 0;
        }

        public string StartBreakdown()
        {
            var parts = new List<string>();
            foreach (var name in _startOrder)
                parts.Add($"{name}: {_startSuccesses[name]}/{_startAttempts[name]}");

            return string.Join(", ", parts);
        }

        private void CountStart(string startName, bool succeeded)
        {
            if (!_startAttempts.ContainsKey(startName))
            {
                _startOrder.Add(startName);
                _startAttempts[startName] = 0;
                _startSuccesses[startName] = 0;
            }

            _startAttempts[startName]++;
            if (succeeded)
                _startSuccesses[startName]++;
        }

        public string Summary()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} denemede {1} başarılı (%{2:0}), ortalama süre {3:0.0} sn",
                Attempts, Successes, SuccessRate * 100f, AverageSuccessSeconds);
        }

        public string Details()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "başarısız {0}, süre aşımı {1}, iptal {2}",
                Failures, TimeOuts, Cancels);
        }
    }
}
