using System;

namespace BuyuFisiltisi.Core.Events
{
    /// <summary>
    /// Bileşenlerin birbirini doğrudan tanımadan haberleşmesini sağlayan olay yolu.
    /// Fısıl (Companion) ve Yaşlı Meşe (LLM) birbirine referans vermez; ikisi de yalnızca bu arayüzü bilir.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>T tipindeki olaylar yayınlandığında çağrılacak dinleyiciyi kaydeder.</summary>
        void Subscribe<T>(Action<T> handler);

        /// <summary>Daha önce kaydedilmiş dinleyiciyi kaldırır.</summary>
        void Unsubscribe<T>(Action<T> handler);

        /// <summary>Olayı, o tipe abone olan tüm dinleyicilere iletir.</summary>
        void Publish<T>(T gameEvent);
    }
}
