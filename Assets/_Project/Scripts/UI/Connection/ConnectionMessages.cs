using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;

namespace BuyuFisiltisi.UI.Connection
{
    public static class ConnectionMessages
    {
        public const string PanelTitle = "Orman sessiz…";
        public const string RetryLabel = "Tekrar dene";
        public const string BusyLabel = "Bağlanıyor…";

        public const string ConnectedIndicator = "● Ollama bağlı";
        public const string CheckingIndicator = "● Ollama kontrol ediliyor…";
        public const string DisconnectedIndicator = "● Ollama bağlantısı yok";
        public const string UnknownIndicator = "● Ollama durumu bilinmiyor";

        public const string UnreachableDetail = "Bağlantı kurulamadı. Ollama uygulamasının açık olduğundan emin ol.";
        public const string TimeoutDetail = "Yaşlı Meşe çok uzun süre sessiz kaldı. Bağlantı zaman aşımına uğradı.";

        public static string ModelMissingDetail(string model)
        {
            var name = string.IsNullOrWhiteSpace(model) ? "yapay zekâ" : model;
            return $"Ollama açık ama {name} modeli bulunamadı. Terminalde şunu çalıştır: ollama pull {name}";
        }

        public static ConnectionView For(OllamaHealth health, string model)
        {
            switch (health)
            {
                case OllamaHealth.Ready:
                    return new ConnectionView(ConnectedIndicator, ConnectionTone.Good, PanelVisibility.Hidden, null, false);
                case OllamaHealth.Checking:
                    return new ConnectionView(CheckingIndicator, ConnectionTone.Neutral, PanelVisibility.Unchanged, null, true);
                case OllamaHealth.ModelMissing:
                    return new ConnectionView(DisconnectedIndicator, ConnectionTone.Bad, PanelVisibility.Shown, ModelMissingDetail(model), false);
                case OllamaHealth.Unreachable:
                    return new ConnectionView(DisconnectedIndicator, ConnectionTone.Bad, PanelVisibility.Shown, UnreachableDetail, false);
                default:
                    return new ConnectionView(UnknownIndicator, ConnectionTone.Neutral, PanelVisibility.Unchanged, null, false);
            }
        }

        public static ConnectionView For(OakDialogueStatus status)
        {
            switch (status)
            {
                case OakDialogueStatus.ServerUnavailable:
                    return new ConnectionView(DisconnectedIndicator, ConnectionTone.Bad, PanelVisibility.Shown, UnreachableDetail, false);
                case OakDialogueStatus.Timeout:
                    return new ConnectionView(DisconnectedIndicator, ConnectionTone.Bad, PanelVisibility.Shown, TimeoutDetail, false);
                case OakDialogueStatus.Ok:
                case OakDialogueStatus.InvalidResponse:
                    return new ConnectionView(ConnectedIndicator, ConnectionTone.Good, PanelVisibility.Unchanged, null, false);
                default:
                    return new ConnectionView(UnknownIndicator, ConnectionTone.Neutral, PanelVisibility.Unchanged, null, false);
            }
        }
    }
}
