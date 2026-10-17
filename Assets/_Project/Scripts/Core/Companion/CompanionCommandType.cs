namespace BuyuFisiltisi.Core.Companion
{
    /// <summary>Oyuncunun Fısıl'a verebileceği emir türleri.</summary>
    public enum CompanionCommandType
    {
        /// <summary>"Yanıma gel": oyuncuyu takip et.</summary>
        FollowPlayer,
        /// <summary>"Bekle": olduğun yerde dur.</summary>
        Wait,
        /// <summary>"Şalteri çek": hedef nesneye git ve etkileş.</summary>
        InteractWithTarget,
        /// <summary>"Çiçeği getir": hedefteki eşyayı al ve oyuncuya getir.</summary>
        FetchItem
    }
}
