using System;
using System.Collections.Generic;

namespace BuyuFisiltisi.Core.Progress
{
    public sealed class QuestLog
    {
        private readonly List<QuestStep> _steps = new List<QuestStep>();
        private readonly HashSet<string> _done = new HashSet<string>();

        public QuestLog(IEnumerable<QuestStep> steps)
        {
            if (steps == null)
                throw new ArgumentNullException(nameof(steps));

            foreach (var step in steps)
            {
                if (string.IsNullOrEmpty(step.Id))
                    throw new ArgumentException("Görev adımının kimliği boş olamaz.", nameof(steps));

                _steps.Add(step);
            }
        }

        public int TotalCount => _steps.Count;
        public int CompletedCount => _done.Count;
        public bool IsFinished => _done.Count == _steps.Count;

        public QuestStep? Current
        {
            get
            {
                foreach (var step in _steps)
                {
                    if (!_done.Contains(step.Id))
                        return step;
                }

                return null;
            }
        }

        public bool IsDone(string id)
        {
            return !string.IsNullOrEmpty(id) && _done.Contains(id);
        }

        public bool Complete(string id)
        {
            if (string.IsNullOrEmpty(id) || _done.Contains(id))
                return false;

            foreach (var step in _steps)
            {
                if (step.Id != id)
                    continue;

                _done.Add(id);
                return true;
            }

            return false;
        }

        public string GetText(string id)
        {
            foreach (var step in _steps)
            {
                if (step.Id == id)
                    return step.Text;
            }

            return string.Empty;
        }
    }
}
