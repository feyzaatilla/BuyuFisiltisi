using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Interaction;
using UnityEngine;

namespace BuyuFisiltisi.Testing
{
    public sealed class CompanionTrialTarget : MonoBehaviour, IInteractable
    {
        public const string DefaultId = "fisil_test_hedefi";

        [SerializeField] private string _interactionId = DefaultId;
        [SerializeField] private string _displayName = "Fısıl Test Hedefi";

        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => transform;
        public Transform FocusPoint => transform;
        public bool IsDone { get; private set; }
        public int InteractCount { get; private set; }
        public string TaskName => $"{CompanionCommandType.InteractWithTarget}:{_interactionId}";

        public bool CanInteract(GameObject interactor)
        {
            return !IsDone;
        }

        public void Interact(GameObject interactor)
        {
            if (IsDone)
                return;

            IsDone = true;
            InteractCount++;
        }

        public void ResetForTrial()
        {
            IsDone = false;
        }
    }
}
