using System.Collections.Generic;

namespace BuyuFisiltisi.Core.Items
{
    public static class ItemCatalog
    {
        public const string SilverHyacinth = "gumus_sumbul";
        public const string NightViolet = "gece_menekse";
        public const string RedWhistleWeed = "kizil_duduk_otu";
        public const string MistFlower = "sis_cicegi";
        public const string RootPomegranate = "kok_nari";

        public static readonly IReadOnlyList<string> Flowers = new[]
        {
            SilverHyacinth, NightViolet, RedWhistleWeed, MistFlower, RootPomegranate
        };

        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            { SilverHyacinth, "Gümüş Sümbül" },
            { NightViolet, "Gece Menekşesi" },
            { RedWhistleWeed, "Kızıl Düdük Otu" },
            { MistFlower, "Sis Çiçeği" },
            { RootPomegranate, "Kök Narı" },
            { Recipe.LightTeaId, "Işık Çayı" },
            { Recipe.SilenceSalveId, "Sessizlik Merhemi" },
            { Recipe.WhisperEssenceId, "Fısıltı Özü" }
        };

        public static bool IsFlower(string itemId)
        {
            foreach (var flower in Flowers)
            {
                if (flower == itemId)
                    return true;
            }

            return false;
        }

        public static string GetName(string itemId)
        {
            return itemId != null && Names.TryGetValue(itemId, out var name) ? name : itemId;
        }
    }
}
