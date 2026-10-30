using System.Text;
using BuyuFisiltisi.Core.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    [RequireComponent(typeof(PlayerInventory))]
    public sealed class InventoryHud : MonoBehaviour
    {
        private readonly StringBuilder _text = new StringBuilder();
        private PlayerInventory _inventory;
        private GUIStyle _style;

        private void OnGUI()
        {
            if (_inventory == null)
                _inventory = GetComponent<PlayerInventory>();

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.LowerLeft };
                _style.normal.textColor = new Color(1f, 0.95f, 0.6f);
            }

            _text.Length = 0;
            foreach (var flower in ItemCatalog.Flowers)
                Append(flower);
            foreach (var recipe in Recipe.All)
                Append(recipe.ProductId);

            if (_text.Length == 0)
                _text.Append("Çanta boş");

            GUI.Label(new Rect(20f, Screen.height - 260f, 400f, 240f), _text.ToString(), _style);
        }

        private void Append(string itemId)
        {
            var count = _inventory.Inventory.Count(itemId);
            if (count <= 0)
                return;

            if (_text.Length > 0)
                _text.Append('\n');
            _text.Append(ItemCatalog.GetName(itemId)).Append(": ").Append(count);
        }
    }
}
