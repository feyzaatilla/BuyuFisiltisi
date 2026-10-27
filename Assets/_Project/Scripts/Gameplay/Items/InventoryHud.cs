using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    public sealed class InventoryHud : MonoBehaviour, IRequiresEventBus
    {
        private IEventBus _eventBus;
        private int _flowerCount;
        private GUIStyle _style;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
        }

        private void OnItemCollected(ItemCollectedEvent gameEvent)
        {
            if (gameEvent.ItemId == CollectibleFlower.LightFlowerId)
                _flowerCount = gameEvent.Total;
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                _style.normal.textColor = new Color(1f, 0.95f, 0.6f);
            }

            GUI.Label(new Rect(20f, Screen.height - 60f, 500f, 50f), "Işık çiçeği: " + _flowerCount, _style);
        }
    }
}
