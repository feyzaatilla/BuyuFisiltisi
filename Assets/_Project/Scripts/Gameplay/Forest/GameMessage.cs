using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class GameMessage : MonoBehaviour
    {
        private static GameMessage _instance;

        private string _text = string.Empty;
        private Color _color = Color.white;
        private float _until;
        private GUIStyle _style;

        public static void Show(string text, Color color, float seconds = 3f)
        {
            if (_instance == null)
            {
                var host = new GameObject("EkranMesaji") { hideFlags = HideFlags.HideAndDontSave };
                _instance = host.AddComponent<GameMessage>();
            }

            _instance._text = text;
            _instance._color = color;
            _instance._until = Time.unscaledTime + seconds;
        }

        public static void Show(string text, float seconds = 3f)
        {
            Show(text, new Color(1f, 0.85f, 0.5f), seconds);
        }

        private void OnGUI()
        {
            if (Time.timeScale == 0f || Time.unscaledTime > _until || string.IsNullOrEmpty(_text))
                return;

            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };

            var fade = Mathf.Clamp01((_until - Time.unscaledTime) / 0.4f);
            _style.normal.textColor = new Color(_color.r, _color.g, _color.b, fade);
            GUI.Label(new Rect(40f, Screen.height * 0.24f, Screen.width - 80f, 70f), _text, _style);
        }
    }
}
