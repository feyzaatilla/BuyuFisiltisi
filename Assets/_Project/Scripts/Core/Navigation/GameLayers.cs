namespace BuyuFisiltisi.Core.Navigation
{
    /// <summary>
    /// Projede kullanılan fizik katmanları. Adlar ve sıra numaraları Tags and Layers ayarıyla birebir aynı olmalıdır;
    /// "Büyü Fısıltısı → Proje Kurulum Kontrolü" penceresi bunu doğrular ve eksikleri ekler.
    /// </summary>
    public static class GameLayers
    {
        public const int Ground = 6;
        public const int Player = 7;
        public const int Companion = 8;
        public const int Interactable = 9;
        public const int Obstacle = 10;

        public const string GroundName = "Ground";
        public const string PlayerName = "Player";
        public const string CompanionName = "Companion";
        public const string InteractableName = "Interactable";
        public const string ObstacleName = "Obstacle";

        /// <summary>Katman numarası → katman adı eşlemesi (kurulum aracı ve testler kullanır).</summary>
        public static readonly (int Index, string Name)[] All =
        {
            (Ground, GroundName),
            (Player, PlayerName),
            (Companion, CompanionName),
            (Interactable, InteractableName),
            (Obstacle, ObstacleName),
        };

        /// <summary>Verilen katman numarası için LayerMask değeri.</summary>
        public static int Mask(int layer) => 1 << layer;

        /// <summary>Fısıl'ın engel sensörünün çarpacağı katmanlar (Aşama 2'de ObstacleSensor kullanır).</summary>
        public static int ObstacleSensorMask => Mask(Ground) | Mask(Obstacle) | Mask(Interactable);
    }
}
