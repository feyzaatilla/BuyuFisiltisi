using BuyuFisiltisi.Core.Companion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Items
{
    public sealed class FlowerFetchKeyboardInput : MonoBehaviour, ICompanionCommandIssuer
    {
        [SerializeField] private CollectibleFlower[] _flowers = new CollectibleFlower[0];

        private ICompanionCommandReceiver _receiver;

        public void SetCommandReceiver(ICompanionCommandReceiver receiver)
        {
            _receiver = receiver;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _receiver == null)
                return;

            if (!keyboard.vKey.wasPressedThisFrame)
                return;

            var flower = FindNextFlower();
            if (flower != null)
                _receiver.IssueCommand(CompanionCommand.FetchItem(flower));
        }

        private CollectibleFlower FindNextFlower()
        {
            foreach (var flower in _flowers)
            {
                if (flower != null && flower.CanInteract(gameObject))
                    return flower;
            }

            return null;
        }
    }
}