using System;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.UI.Menu;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.UI.Pause
{
    public sealed class PauseMenuView : MonoBehaviour, IRequiresEventBus
    {
        public const string PausedTitle = "Oyun duraklatıldı";
        public const string ResumeLabel = "Devam Et";
        public const string MainMenuLabel = "Ana Menü";
        public const string ResumeHint = "Esc ya da P: devam";

        private readonly PauseState _state = new PauseState();

        private IEventBus _bus;
        private Func<bool> _otherWindowOpen;
        private bool _dialogueActive;
        private bool _blockedLastFrame;

        private GUIStyle _titleStyle;
        private GUIStyle _textStyle;
        private GUIStyle _keyStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _buttonStyle;
        private Texture2D _shade;
        private float _width;
        private float _height;

        public bool IsPaused => _state.IsPaused;

        public void InjectEventBus(IEventBus eventBus)
        {
            _bus?.Unsubscribe<DialogueFocusChangedEvent>(OnDialogueFocus);
            _bus = eventBus;
            _bus?.Subscribe<DialogueFocusChangedEvent>(OnDialogueFocus);
        }

        public void SetWindowCheck(Func<bool> otherWindowOpen)
        {
            _otherWindowOpen = otherWindowOpen;
        }

        private void OnDestroy()
        {
            _bus?.Unsubscribe<DialogueFocusChangedEvent>(OnDialogueFocus);
            if (_state.IsPaused)
                Time.timeScale = 1f;
        }

        private void OnDialogueFocus(DialogueFocusChangedEvent focus)
        {
            _dialogueActive = focus.IsActive;
        }

        private bool IsBlockedNow()
        {
            return _dialogueActive || (_otherWindowOpen != null && _otherWindowOpen());
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            var escape = keyboard.escapeKey.wasPressedThisFrame;
            var pauseKey = keyboard.pKey.wasPressedThisFrame;
            if (!escape && !pauseKey)
                return;

            var blocked = _blockedLastFrame || IsBlockedNow();
            var frozen = !_state.IsPaused && Time.timeScale == 0f;
            if (_state.OnKey(escape, blocked, frozen))
                Apply();
        }

        private void LateUpdate()
        {
            _blockedLastFrame = IsBlockedNow();
        }

        private void Apply()
        {
            Time.timeScale = _state.IsPaused ? 0f : 1f;
        }

        private void Resume()
        {
            _state.Resume();
            Apply();
        }

        private void GoToMainMenu()
        {
            _state.Resume();
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuContent.MenuSceneName);
        }

        private static void Quit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGUI()
        {
            if (!_state.IsPaused)
                return;

            EnsureStyles();
            GUI.depth = -200;
            var scale = MainMenuContent.UiScale(Screen.height);
            _width = Screen.width / scale;
            _height = Screen.height / scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.DrawTexture(new Rect(0f, 0f, _width, _height), _shade);

            var width = Mathf.Min(760f, _width - 40f);
            var x = (_width - width) / 2f;

            if (_state.ShowHelp)
                DrawHelp(x, width);
            else
                DrawMain(x, width);

            GUI.Label(new Rect(0f, _height - 44f, _width, 30f), ResumeHint, _hintStyle);
        }

        private void DrawMain(float x, float width)
        {
            var y = _height * 0.22f;
            GUI.Label(new Rect(x, y, width, 64f), PausedTitle, _titleStyle);
            y += 100f;

            var buttonWidth = 260f;
            var bx = (_width - buttonWidth) / 2f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), ResumeLabel, _buttonStyle))
                Resume();
            y += 66f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuContent.HowToLabel, _buttonStyle))
                _state.OpenHelp();
            y += 66f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuLabel, _buttonStyle))
                GoToMainMenu();
            y += 66f;
            if (GUI.Button(new Rect(bx, y, buttonWidth, 54f), MainMenuContent.QuitLabel, _buttonStyle))
                Quit();
        }

        private void DrawHelp(float x, float width)
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
                _state.CloseHelp();
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _titleStyle.normal.textColor = new Color(0.75f, 0.95f, 0.7f);
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
            _textStyle.normal.textColor = Color.white;
            _keyStyle = new GUIStyle(_textStyle) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };
            _keyStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            _hintStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            _shade = new Texture2D(1, 1);
            _shade.SetPixel(0, 0, new Color(0.02f, 0.05f, 0.04f, 0.85f));
            _shade.Apply();
        }
    }
}
