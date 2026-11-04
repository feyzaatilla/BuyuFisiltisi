using BuyuFisiltisi.LLM.Ollama;
using BuyuFisiltisi.UI.Connection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.UI.Menu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private OllamaConnectionMonitor _monitor;
        [SerializeField] private string _gameScene = MainMenuContent.GameSceneName;

        private readonly MainMenuState _state = new MainMenuState();

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _textStyle;
        private GUIStyle _keyStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _shadowStyle;
        private Texture2D _shade;
        private float _width;
        private float _height;

        public MainMenuPage Page => _state.Page;

        private void Awake()
        {
            if (_monitor == null)
                _monitor = GetComponent<OllamaConnectionMonitor>();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                _state.HandleEscape();
        }

        public void Play()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_gameScene);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGUI()
        {
            EnsureStyles();
            var scale = MainMenuContent.UiScale(Screen.height);
            _width = Screen.width / scale;
            _height = Screen.height / scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            Shade(new Rect(0f, 0f, _width, _height), _state.Page == MainMenuPage.HowTo ? 0.82f : 0.18f);

            var width = Mathf.Min(760f, _width - 40f);
            var x = (_width - width) / 2f;

            if (_state.Page == MainMenuPage.HowTo)
                DrawHowTo(x, width);
            else
                DrawMain(x, width);

            DrawStatus();
        }

        private void DrawMain(float x, float width)
        {
            var y = _height * 0.2f;
            Shade(new Rect(x + width * 0.15f, y - 20f, width * 0.7f, 380f), 0.45f);
            GUI.Label(new Rect(x + 3f, y + 3f, width, 70f), MainMenuContent.Title, _shadowStyle);
            GUI.Label(new Rect(x, y, width, 70f), MainMenuContent.Title, _titleStyle);
            y += 80f;
            GUI.Label(new Rect(x, y, width, 34f), MainMenuContent.Subtitle, _subtitleStyle);
            y += 70f;

            var buttonWidth = 260f;
            var bx = (_width - buttonWidth) / 2f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuContent.PlayLabel, _buttonStyle))
                Play();
            y += 66f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuContent.HowToLabel, _buttonStyle))
                _state.OpenHowTo();
            y += 66f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuContent.QuitLabel, _buttonStyle))
                Quit();
        }

        private void DrawHowTo(float x, float width)
        {
            var y = _height * 0.08f;
            GUI.Label(new Rect(x, y, width, 50f), MainMenuContent.HowToLabel, _titleStyle);
            y += 64f;

            foreach (var goal in MainMenuContent.Goals)
            {
                GUI.Label(new Rect(x, y, width, 28f), "• " + goal, _textStyle);
                y += 28f;
            }

            y += 18f;
            var keyWidth = width * 0.38f;
            foreach (var hint in MainMenuContent.Controls)
            {
                GUI.Label(new Rect(x, y, keyWidth, 28f), hint.Key, _keyStyle);
                GUI.Label(new Rect(x + keyWidth + 16f, y, width - keyWidth - 16f, 28f), hint.Action, _textStyle);
                y += 28f;
            }

            y += 24f;
            var buttonWidth = 200f;
            if (GUI.Button(new Rect((_width - buttonWidth) / 2f, y, buttonWidth, 48f), MainMenuContent.BackLabel, _buttonStyle))
                _state.Back();
        }

        private void DrawStatus()
        {
            var health = _monitor != null ? _monitor.Health : OllamaHealth.Unknown;
            var model = _monitor != null ? _monitor.Model : null;

            _statusStyle.normal.textColor = ToneColor(MainMenuContent.Tone(health, model));
            var y = _height - 46f;
            GUI.Label(new Rect(20f, y, 420f, 30f), MainMenuContent.Indicator(health, model), _statusStyle);

            if (_monitor != null && MainMenuContent.CanRetry(health))
            {
                if (GUI.Button(new Rect(_width - 180f, y - 4f, 160f, 36f), MainMenuContent.RetryLabel))
                    _monitor.Retry();
            }

            var note = MainMenuContent.Note(health, model);
            if (!string.IsNullOrEmpty(note))
                GUI.Label(new Rect(20f, y - 34f, _width - 40f, 30f), note, _textStyle);
        }

        private void Shade(Rect rect, float alpha)
        {
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(rect, _shade);
            GUI.color = previous;
        }

        private static Color ToneColor(ConnectionTone tone)
        {
            switch (tone)
            {
                case ConnectionTone.Good:
                    return new Color(0.5f, 0.95f, 0.55f);
                case ConnectionTone.Bad:
                    return new Color(1f, 0.45f, 0.4f);
                default:
                    return new Color(0.85f, 0.85f, 0.85f);
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 52, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleStyle.normal.textColor = new Color(0.75f, 0.95f, 0.7f);
            _shadowStyle = new GUIStyle(_titleStyle);
            _shadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            _subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            _subtitleStyle.normal.textColor = new Color(0.9f, 0.9f, 0.8f);
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
            _textStyle.normal.textColor = Color.white;
            _keyStyle = new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };
            _keyStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            _statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            _shade = new Texture2D(1, 1);
            _shade.SetPixel(0, 0, new Color(0.02f, 0.06f, 0.05f, 1f));
            _shade.Apply();
        }
    }
}
