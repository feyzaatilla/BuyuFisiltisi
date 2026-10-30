using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Book
{
    public sealed class CauldronStation : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _player;
        [SerializeField] private SpellBookView _book;
        [SerializeField] private float _radius = 4.5f;

        private Brewer _brewer;
        private bool _near;
        private bool _open;
        private string _note = string.Empty;
        private GUIStyle _promptStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _textStyle;
        private GUIStyle _buttonStyle;
        private Texture2D _rim;
        private Texture2D _panel;

        public bool IsOpen => _open;

        public void InjectEventBus(IEventBus eventBus)
        {
            _brewer = eventBus != null && _inventory != null ? new Brewer(_inventory.Inventory, eventBus) : null;
        }

        private void Update()
        {
            if (_player == null)
                return;

            var offset = _player.position - transform.position;
            offset.y = 0f;
            _near = offset.sqrMagnitude <= _radius * _radius;
            if (!_near && _open)
                _open = false;

            if (Time.timeScale == 0f)
                return;

            var keyboard = Keyboard.current;
            if (_open && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                _open = false;
                return;
            }

            var mouse = Mouse.current;
            var bookOpen = _book != null && _book.IsOpen;
            if (!_open && _near && !bookOpen && mouse != null && mouse.leftButton.wasPressedThisFrame && IsPointedAt(mouse.position.ReadValue()))
            {
                _open = true;
                _note = string.Empty;
            }
        }

        private bool IsPointedAt(Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera == null)
                return false;

            foreach (var hit in Physics.RaycastAll(camera.ScreenPointToRay(screenPoint), 60f, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.transform.IsChildOf(transform))
                    return true;
            }

            return false;
        }

        private void OnGUI()
        {
            if (Time.timeScale == 0f || !_near)
                return;

            EnsureStyles();

            if (!_open)
            {
                if (_book == null || !_book.IsOpen)
                    GUI.Label(new Rect(0f, Screen.height * 0.6f, Screen.width, 30f), "Kazana tıkla, demlemeye başla", _promptStyle);
                return;
            }

            var width = Mathf.Min(620f, Screen.width - 40f);
            var height = 150f + Recipe.All.Count * 74f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUI.DrawTexture(new Rect(x - 10f, y - 10f, width + 20f, height + 20f), _rim);
            GUI.DrawTexture(new Rect(x, y, width, height), _panel);
            GUI.Label(new Rect(x, y + 12f, width, 36f), "Büyü Kazanı", _titleStyle);

            var rowY = y + 60f;
            foreach (var recipe in Recipe.All)
            {
                var parts = new List<string>();
                foreach (var ingredient in recipe.Ingredients)
                {
                    var have = _inventory != null ? _inventory.Inventory.Count(ingredient.ItemId) : 0;
                    parts.Add(ItemCatalog.GetName(ingredient.ItemId) + " " + have + "/" + ingredient.Amount);
                }

                GUI.Label(new Rect(x + 24f, rowY, width - 210f, 54f), ItemCatalog.GetName(recipe.ProductId) + "\n" + string.Join(" + ", parts), _textStyle);

                GUI.enabled = _brewer != null && _brewer.CanBrew(recipe);
                if (GUI.Button(new Rect(x + width - 180f, rowY + 8f, 156f, 40f), "Demle", _buttonStyle) && _brewer.TryBrew(recipe))
                    _note = ItemCatalog.GetName(recipe.ProductId) + " kazanda köpürdü ve hazır!";
                GUI.enabled = true;
                rowY += 74f;
            }

            GUI.Label(new Rect(x + 24f, y + height - 76f, width - 48f, 28f), _note, _textStyle);
            if (GUI.Button(new Rect(x + width - 140f, y + height - 50f, 116f, 36f), "Kapat", _buttonStyle))
                _open = false;
        }

        private void EnsureStyles()
        {
            if (_promptStyle != null)
                return;

            _promptStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _promptStyle.normal.textColor = new Color(0.6f, 1f, 0.65f);
            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleStyle.normal.textColor = new Color(0.32f, 0.1f, 0.55f);
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            _textStyle.normal.textColor = new Color(0.32f, 0.1f, 0.55f);
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
            _rim = Solid(new Color(0.25f, 0.08f, 0.4f, 0.95f));
            _panel = Solid(new Color(0.95f, 0.9f, 0.76f, 0.97f));
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
