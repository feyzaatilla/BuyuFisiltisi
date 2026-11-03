using BuyuFisiltisi.LLM.Ollama;
using BuyuFisiltisi.UI.Connection;

namespace BuyuFisiltisi.UI.Menu
{
    public readonly struct ControlHint
    {
        public ControlHint(string key, string action)
        {
            Key = key;
            Action = action;
        }

        public string Key { get; }
        public string Action { get; }
    }

    public static class MainMenuContent
    {
        public const string GameSceneName = "ForestScene";
        public const string Title = "Büyü Fısıltısı";
        public const string Subtitle = "Sessiz ormanı sözlerinle ve iyiliklerinle uyandır.";
        public const string PlayLabel = "Oyna";
        public const string HowToLabel = "Nasıl Oynanır";
        public const string QuitLabel = "Çıkış";
        public const string BackLabel = "Geri";
        public const string RetryLabel = ConnectionMessages.RetryLabel;
        public const float ReferenceHeight = 1080f;
        public const string OfflineNote = "Ollama kapalıyken Yaşlı Meşe konuşamaz. Oyuna yine de girebilirsin.";

        public static readonly string[] Goals =
        {
            "Kara bulutları rüzgâr büyüsüyle dağıt.",
            "Çiçek Vadisi'nde çiçek topla, kazanda iksir demle.",
            "Ateş böceğini ve Yaşlı Meşe'nin kökünü iyileştir.",
            "Gölge Tilki'yi sakinleştir, gizli kök mağarasını geç.",
            "Yaşlı Meşe'yi ikna et ve Orman Kalbi'nin kapısını aç."
        };

        public static readonly ControlHint[] Controls =
        {
            new ControlHint("W A S D / Oklar", "Yürü"),
            new ControlHint("Shift", "Koş"),
            new ControlHint("Space", "Zıpla"),
            new ControlHint("Sol tık / R", "Rüzgâr büyüsü"),
            new ControlHint("E", "Yaşlı Meşe ile konuş"),
            new ControlHint("Tab", "Fısıl'ın sayfalarını aç"),
            new ControlHint("F / G", "Fısıl: yanıma gel / bekle"),
            new ControlHint("Esc", "Pencereyi kapat")
        };

        public static float UiScale(float screenHeight)
        {
            return screenHeight > 0f ? screenHeight / ReferenceHeight : 1f;
        }

        public static string Indicator(OllamaHealth health, string model)
        {
            return ConnectionMessages.For(health, model).Indicator;
        }

        public static ConnectionTone Tone(OllamaHealth health, string model)
        {
            return ConnectionMessages.For(health, model).Tone;
        }

        public static bool CanRetry(OllamaHealth health)
        {
            return health == OllamaHealth.Unreachable || health == OllamaHealth.ModelMissing;
        }

        public static string Note(OllamaHealth health, string model)
        {
            switch (health)
            {
                case OllamaHealth.Unreachable:
                    return OfflineNote;
                case OllamaHealth.ModelMissing:
                    return ConnectionMessages.ModelMissingDetail(model);
                default:
                    return null;
            }
        }
    }
}
