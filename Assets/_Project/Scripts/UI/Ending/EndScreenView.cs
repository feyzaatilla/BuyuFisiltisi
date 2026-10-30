using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Progress;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.UI.Ending
{
    public sealed class EndScreenView : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private float _delaySeconds = 2f;

        private readonly EndingState _ending = new EndingState();
        private readonly List<string> _deeds = new List<string>();
        private IEventBus _eventBus;
        private float _showAt = -1f;
        private bool _shown;
        private GUIStyle _titleStyle;
        private GUIStyle _textStyle;
        private GUIStyle _buttonStyle;
        private Texture2D _shade;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (_shown)
                Time.timeScale = 1f;
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            Unsubscribe();
            _eventBus = eventBus;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void Subscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Subscribe<DoorOpenedEvent>(OnDoorOpened);
            _eventBus.Subscribe<OakAttackedEvent>(OnOakAttacked);
            _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);
            _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void Unsubscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<DoorOpenedEvent>(OnDoorOpened);
            _eventBus.Unsubscribe<OakAttackedEvent>(OnOakAttacked);
            _eventBus.Unsubscribe<GameEndedEvent>(OnGameEnded);
            _eventBus.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDoorOpened(DoorOpenedEvent opened)
        {
            End(GameOutcome.Good);
        }

        private void OnOakAttacked(OakAttackedEvent attacked)
        {
            End(GameOutcome.Bad);
        }

        private void OnGameEnded(GameEndedEvent ended)
        {
            End(ended.Ending == GameEnding.Good ? GameOutcome.Good : GameOutcome.Bad);
        }

        private void OnDeedRecorded(DeedRecordedEvent deed)
        {
            if (!_ending.IsOver && !_deeds.Contains(deed.Description))
                _deeds.Add(deed.Description);
        }

        private void End(GameOutcome outcome)
        {
            if (!_ending.TryEnd(outcome))
                return;

            _showAt = Time.unscaledTime + _delaySeconds;
            Debug.Log("[Bitiş] " + _ending.Title);
        }

        private void Update()
        {
            if (_shown || _showAt < 0f || Time.unscaledTime < _showAt)
                return;

            _shown = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnGUI()
        {
            if (!_shown)
                return;

            EnsureStyles();
            GUI.depth = -100;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _shade);

            var width = Mathf.Min(720f, Screen.width - 40f);
            var x = (Screen.width - width) / 2f;
            var y = Screen.height * 0.18f;

            _titleStyle.normal.textColor = _ending.Outcome == GameOutcome.Good
                ? new Color(1f, 0.9f, 0.5f)
                : new Color(1f, 0.45f, 0.4f);
            GUI.Label(new Rect(x, y, width, 60f), _ending.Title, _titleStyle);
            y += 80f;

            GUI.Label(new Rect(x, y, width, 70f), _ending.Story, _textStyle);
            y += 90f;

            if (_deeds.Count > 0)
            {
                GUI.Label(new Rect(x, y, width, 30f), "Yaptığın iyilikler:", _textStyle);
                y += 34f;
                foreach (var deed in _deeds)
                {
                    GUI.Label(new Rect(x, y, width, 30f), "• " + deed, _textStyle);
                    y += 30f;
                }
            }

            y += 30f;
            var buttonWidth = 200f;
            if (GUI.Button(new Rect(Screen.width / 2f - buttonWidth - 10f, y, buttonWidth, 50f), "Yeniden Oyna", _buttonStyle))
                Restart();

            if (GUI.Button(new Rect(Screen.width / 2f + 10f, y, buttonWidth, 50f), "Çıkış", _buttonStyle))
                Quit();
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.UpperCenter, wordWrap = true };
            _textStyle.normal.textColor = Color.white;
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
            _shade = new Texture2D(1, 1);
            _shade.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
            _shade.Apply();
        }

        private static void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
    }
}
