using System.Collections.Generic;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakDeedCatalog
    {
        private static readonly string[] Subjects =
        {
            "bulut",
            "ateş böce",
            "kök",
            "tilki",
            "mağara"
        };

        private static readonly string[] Hints =
        {
            "Ormanın üstünde hâlâ kara bulutlar var. Yanlarına git, sol tık ya da R ile rüzgâr büyüsü yap.",
            "Yaralı ateş böceği hâlâ ışıksız. Kazanda Işık Çayı demle ve ona dokun.",
            "Yaralı kökün hâlâ acıyor. Kazanda Sessizlik Merhemi demle ve köküme sür.",
            "Gölgelerde ürkek bir Gölge Tilki dolaşıyor. Ona zarar verme, büyünle sakinleştir.",
            "Gizli bir girdabın altında tuzaklı bir mağara var. Tuzaklardan geç ve taş bulmacayı çöz."
        };

        public static int Count => Subjects.Length;

        public static List<string> MissingHints(IReadOnlyCollection<string> deeds)
        {
            var missing = new List<string>();
            for (var i = 0; i < Subjects.Length; i++)
            {
                if (!IsDone(deeds, OakClaimGuard.Fold(Subjects[i])))
                    missing.Add(Hints[i]);
            }

            return missing;
        }

        private static bool IsDone(IReadOnlyCollection<string> deeds, string subject)
        {
            if (deeds == null)
                return false;

            foreach (var deed in deeds)
            {
                if (OakClaimGuard.Fold(deed).Contains(subject))
                    return true;
            }

            return false;
        }
    }
}
