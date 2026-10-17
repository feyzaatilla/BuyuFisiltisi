namespace BuyuFisiltisi.Core.GameEvents
{
    /// <summary>Bir tarif (ör. Işık Çayı) tamamlandığında yayınlanır.</summary>
    public readonly struct RecipeCompletedEvent
    {
        public readonly string RecipeId;
        public RecipeCompletedEvent(string recipeId) { RecipeId = recipeId; }
    }

    /// <summary>
    /// Bir iyilik oyun tarafından kaydedildiğinde yayınlanır (ör. ateş böceği iyileştirildi).
    /// Bu bir oyun kaydıdır, oyuncunun iddiası değildir.
    /// </summary>
    public readonly struct DeedRecordedEvent
    {
        public readonly string DeedId;
        public readonly string Description;

        public DeedRecordedEvent(string deedId, string description)
        {
            DeedId = deedId;
            Description = description;
        }
    }

    /// <summary>Yaşlı Meşe'nin cadıya duyduğu toplam güven değiştiğinde yayınlanır.</summary>
    public readonly struct TrustChangedEvent
    {
        public readonly int PreviousTrust;
        public readonly int CurrentTrust;
        public readonly int Delta;

        public TrustChangedEvent(int previousTrust, int currentTrust, int delta)
        {
            PreviousTrust = previousTrust;
            CurrentTrust = currentTrust;
            Delta = delta;
        }
    }

    /// <summary>Oyunun hangi sonla bittiği.</summary>
    public enum GameEnding
    {
        /// <summary>Kapı açıldı, cadı Orman Kalbi'ne ulaştı.</summary>
        Good,
        /// <summary>Yaşlı Meşe saldırdı.</summary>
        OakAttacked,
        /// <summary>Cadı ormandan sürüldü.</summary>
        Banished
    }

    /// <summary>Oyun bittiğinde yayınlanır; son ekran bu olayı dinler.</summary>
    public readonly struct GameEndedEvent
    {
        public readonly GameEnding Ending;
        public GameEndedEvent(GameEnding ending) { Ending = ending; }
    }
}
