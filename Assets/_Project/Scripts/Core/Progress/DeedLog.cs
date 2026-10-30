using System.Collections.Generic;

namespace BuyuFisiltisi.Core.Progress
{
    public sealed class DeedLog
    {
        private readonly List<string> _deedIds = new List<string>();
        private readonly Dictionary<string, string> _descriptions = new Dictionary<string, string>();

        public int Count => _deedIds.Count;
        public IReadOnlyList<string> DeedIds => _deedIds;

        public bool Has(string deedId)
        {
            return !string.IsNullOrEmpty(deedId) && _descriptions.ContainsKey(deedId);
        }

        public string GetDescription(string deedId)
        {
            return Has(deedId) ? _descriptions[deedId] : string.Empty;
        }

        public bool Record(string deedId, string description)
        {
            if (string.IsNullOrEmpty(deedId) || Has(deedId))
                return false;

            _deedIds.Add(deedId);
            _descriptions[deedId] = description ?? string.Empty;
            return true;
        }
    }
}