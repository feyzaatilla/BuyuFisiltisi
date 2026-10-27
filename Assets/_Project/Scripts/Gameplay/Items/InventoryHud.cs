using BuyuFisiltisi.Core.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    [RequireComponent(typeof(PlayerInventory))]
    public sealed class InventoryHud : MonoBehaviour
    {
        private PlayerInventory _inventory;
        private GUIStyle _style;

        private void OnGUI()
        {
            if (_inventory == null)
                _inventory = GetComponent<PlayerInventory>();

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                _style.normal.textColor = new Color(1f, 0.95f, 0.6f);
            }

            var flowers = _inventory.Inventory.Count(CollectibleFlower.LightFlowerId);
            var tea = _inventory.Inventory.Count(Recipe.LightTeaId);
            GUI.Label(new Rect(20f, Screen.height - 100f, 600f, 40f), "Işık çiçeği: " + flowers, _style);
            GUI.Label(new Rect(20f, Screen.height - 60f, 600f, 40f), "Işık çayı: " + tea, _style);
        }
    }
}
