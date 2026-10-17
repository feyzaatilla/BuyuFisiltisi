using UnityEngine;

namespace BuyuFisiltisi.Core.Interaction
{
    /// <summary>
    /// Fısıl'ın ya da Cadı'nın etkileşime girebildiği her nesne (şalter, çiçek, yaralı kök...).
    /// Fısıl nesnenin ne olduğunu bilmez; yalnızca nereye gideceğini ve ne zaman etkileşebileceğini bilir.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Nesnenin benzersiz kimliği (ör. "bridge_switch").</summary>
        string InteractionId { get; }

        /// <summary>Arayüzde gösterilecek ad (ör. "Köprü Şalteri").</summary>
        string DisplayName { get; }

        /// <summary>Etkileşim için durulması gereken nokta (NavMesh üzerinde olmalı).</summary>
        Transform InteractionPoint { get; }

        /// <summary>Etkileşim sırasında bakılacak nokta (sensörlerin görüş hattı kontrolü için).</summary>
        Transform FocusPoint { get; }

        /// <summary>Şu an etkileşime açık mı (ör. şalter zaten çekildiyse false).</summary>
        bool CanInteract(GameObject interactor);

        /// <summary>Etkileşimi gerçekleştirir.</summary>
        void Interact(GameObject interactor);
    }
}
