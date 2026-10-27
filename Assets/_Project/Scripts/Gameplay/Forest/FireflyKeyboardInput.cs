using BuyuFisiltisi.Core.Companion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class FireflyKeyboardInput : MonoBehaviour, ICompanionCommandIssuer
    {
        [SerializeField] private Firefly _firefly;

        private ICompanionCommandReceiver _receiver;

        public void SetCommandReceiver(ICompanionCommandReceiver receiver)
        {
            _receiver = receiver;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _receiver == null || _firefly == null)
                return;

            if (!keyboard.gKey.wasPressedThisFrame)
                return;

            if (_firefly.CanInteract(gameObject))
                _receiver.IssueCommand(CompanionCommand.InteractWith(_firefly));
        }
    }
}