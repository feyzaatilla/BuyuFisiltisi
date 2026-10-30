using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Items
{
    [RequireComponent(typeof(PlayerInventory))]
    public sealed class BrewKeyboardInput : MonoBehaviour, IRequiresEventBus
    {
        private Brewer _brewer;

        public void InjectEventBus(IEventBus eventBus)
        {
            var inventory = GetComponent<PlayerInventory>();
            _brewer = new Brewer(inventory.Inventory, eventBus);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _brewer == null)
                return;

            if (!keyboard.tKey.wasPressedThisFrame)
                return;

            var brewedAny = false;
            foreach (var recipe in Recipe.All)
            {
                if (!_brewer.TryBrew(recipe))
                    continue;

                brewedAny = true;
                Debug.Log("[Kazan] " + ItemCatalog.GetName(recipe.ProductId) + " demlendi.");
            }

            if (!brewedAny)
                Debug.Log("[Kazan] Hiçbir tarif için malzeme yetmiyor.");
        }
    }
}
