using System;
using BuyuFisiltisi.Core.Interaction;

namespace BuyuFisiltisi.Core.Companion
{
    /// <summary>
    /// Fısıl'a verilen tek bir emir. Değiştirilemez (immutable); yalnızca fabrika metotlarıyla oluşturulur,
    /// böylece hedefi eksik bir "Şalteri çek" emri oluşturmak mümkün olmaz.
    /// </summary>
    public sealed class CompanionCommand
    {
        public CompanionCommandType Type { get; }

        /// <summary>InteractWithTarget ve FetchItem için hedef; diğer emirlerde null.</summary>
        public IInteractable Target { get; }

        private CompanionCommand(CompanionCommandType type, IInteractable target)
        {
            Type = type;
            Target = target;
        }

        public static CompanionCommand FollowPlayer() => new CompanionCommand(CompanionCommandType.FollowPlayer, null);

        public static CompanionCommand Wait() => new CompanionCommand(CompanionCommandType.Wait, null);

        public static CompanionCommand InteractWith(IInteractable target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return new CompanionCommand(CompanionCommandType.InteractWithTarget, target);
        }

        public static CompanionCommand FetchItem(IInteractable itemSource)
        {
            if (itemSource == null) throw new ArgumentNullException(nameof(itemSource));
            return new CompanionCommand(CompanionCommandType.FetchItem, itemSource);
        }

        public override string ToString() =>
            Target == null ? Type.ToString() : $"{Type} → {Target.InteractionId}";
    }
}
