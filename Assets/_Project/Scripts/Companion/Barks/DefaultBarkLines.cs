using System.Collections.Generic;

namespace BuyuFisiltisi.Companion.Barks
{
    public static class DefaultBarkLines
    {
        public static IReadOnlyDictionary<BarkTrigger, string[]> Create()
        {
            return new Dictionary<BarkTrigger, string[]>
            {
                [BarkTrigger.Following] = new[]
                {
                    "Geliyorum, geliyorum. Sayfalarım buruşmasın yeter.",
                    "Bir cadının peşinden uçmak... Kariyerimin zirvesi.",
                    "Yavaş yürü, ben bir kitabım, kuş değil."
                },
                [BarkTrigger.Waiting] = new[]
                {
                    "Bekliyorum. Kitaplar bunda iyidir.",
                    "Burada mı duracağım? Peki, ilham gelirse haber ver.",
                    "Tozlanmadan dönersen sevinirim."
                },
                [BarkTrigger.GoingToTarget] = new[]
                {
                    "Yine mi ben? Tabii ki yine ben.",
                    "O yarığa senin sığmadığını unutmadım.",
                    "Tamam, tamam. Kahramanlık sırası bende."
                },
                [BarkTrigger.TaskSucceeded] = new[]
                {
                    "Oldu. Alkış beklemiyorum ama reddetmem de.",
                    "Şalter çekildi. Bir teşekkür fena olmazdı.",
                    "Gördün mü? Bir kitap bazen yeter."
                },
                [BarkTrigger.TaskFailed] = new[]
                {
                    "Olmadı. Benim suçum değil, yolun suçu.",
                    "Oraya gidemiyorum. Kanatlarım yok, fark ettin mi?",
                    "Bu sefer olmadı. Bir daha dene, ama kibar ol."
                }
            };
        }
    }
}
