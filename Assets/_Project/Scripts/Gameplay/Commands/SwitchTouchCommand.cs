using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Interaction;
using BuyuFisiltisi.Gameplay.Forest;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Commands
{
    public sealed class SwitchTouchCommand : MonoBehaviour, ICompanionCommandIssuer
    {
        [SerializeField] private MonoBehaviour _switchTarget;
        [SerializeField] private Transform _player;
        [SerializeField] private float _touchRadius = 4f;

        private ICompanionCommandReceiver _receiver;
        private bool _wasNear;
        private float _lastSent = -10f;

        public void SetCommandReceiver(ICompanionCommandReceiver receiver)
        {
            _receiver = receiver;
        }

        private void Update()
        {
            if (!(_switchTarget is IInteractable lever) || _player == null || Time.timeScale == 0f)
                return;

            if (!lever.CanInteract(gameObject))
                return;

            var offset = _player.position - _switchTarget.transform.position;
            offset.y = 0f;
            var near = offset.magnitude <= _touchRadius;
            var mouse = Mouse.current;
            var clicked = mouse != null && mouse.leftButton.wasPressedThisFrame && IsPointedAt(mouse.position.ReadValue());

            if ((near && !_wasNear) || clicked)
                SendFisil(lever);

            _wasNear = near;
        }

        private void SendFisil(IInteractable lever)
        {
            if (_receiver == null || Time.time - _lastSent < 2f)
                return;

            _lastSent = Time.time;
            _receiver.IssueCommand(CompanionCommand.InteractWith(lever));
            GameMessage.Show("Fısıl: Şalter o dar girintide, sen sığmazsın. Ben çekerim!", new Color(0.75f, 0.9f, 1f), 3f);
        }

        private bool IsPointedAt(Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera == null)
                return false;

            foreach (var hit in Physics.RaycastAll(camera.ScreenPointToRay(screenPoint), 60f, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.transform.IsChildOf(_switchTarget.transform))
                    return true;
            }

            return false;
        }
    }
}
