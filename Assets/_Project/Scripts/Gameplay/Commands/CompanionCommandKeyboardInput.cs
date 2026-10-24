using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Commands
{
    public sealed class CompanionCommandKeyboardInput : MonoBehaviour, ICompanionCommandIssuer
    {
        [SerializeField] private MonoBehaviour _switchTarget;

        private ICompanionCommandReceiver _receiver;

        public string LastCommand { get; private set; } = "Yok";
        public bool HasReceiver => _receiver != null;

        public void SetCommandReceiver(ICompanionCommandReceiver receiver)
        {
            _receiver = receiver;
        }

        private void OnValidate()
        {
            if (_switchTarget != null && !(_switchTarget is IInteractable))
            {
                Debug.LogWarning("[Emir] Şalter hedefi IInteractable olmalı.", this);
                _switchTarget = null;
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _receiver == null)
                return;

            if (keyboard.fKey.wasPressedThisFrame)
                Send(CompanionCommand.FollowPlayer(), "Yanıma gel");
            else if (keyboard.gKey.wasPressedThisFrame)
                Send(CompanionCommand.Wait(), "Bekle");
            else if (keyboard.bKey.wasPressedThisFrame && _switchTarget is IInteractable target)
                Send(CompanionCommand.InteractWith(target), "Şalteri çek");
        }

        private void Send(CompanionCommand command, string label)
        {
            _receiver.IssueCommand(command);
            LastCommand = label;
        }
    }
}
