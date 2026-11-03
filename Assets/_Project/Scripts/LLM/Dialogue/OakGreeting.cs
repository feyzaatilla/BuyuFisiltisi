using System.Collections.Generic;
using System.Text;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakGreeting
    {
        public const int DeedTrustBonus = 1;
        public const int MaxListedDeeds = 3;
        public const int MaxDeedTrust = 1;

        public static int DeedBonus(OakTrust trust)
        {
            if (trust == null)
                return 0;

            return trust.Current + DeedTrustBonus <= MaxDeedTrust ? DeedTrustBonus : 0;
        }

        public static string Build(IReadOnlyList<string> deeds, OakTrust trust)
        {
            var known = new List<string>();
            if (deeds != null)
            {
                foreach (var deed in deeds)
                {
                    if (!string.IsNullOrWhiteSpace(deed))
                        known.Add(deed.Trim());
                }
            }

            if (known.Count == 0)
                return "Hoş geldin küçük cadı. Ormanım için yaptığın bir iyiliği henüz görmedim.";

            var text = new StringBuilder(trust != null && trust.CanOpenDoor
                ? "Hoş geldin küçük cadı, artık sana güveniyorum. Yaptıklarını gördüm:"
                : "Hoş geldin küçük cadı. Yaptığın iyilikleri unutmadım:");

            var start = known.Count > MaxListedDeeds ? known.Count - MaxListedDeeds : 0;
            for (var i = start; i < known.Count; i++)
                text.Append("\n• ").Append(known[i]);

            return text.ToString();
        }
    }
}
