using System;
using System.Threading;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using BuyuFisiltisi.UI.Connection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.UI.Oak
{
    public sealed class OakDialogueController : MonoBehaviour, IRequiresEventBus
    {
        private const string TalkPrompt = "E  ·  Yaşlı Meşe ile konuş";
        private const string ListeningText = "Yaşlı Meşe seni dinliyor. Yaz ve Enter'a bas. Çıkmak için Esc.";
        private const string WakingText = "Yaşlı Meşe uyanıyor, ilk cevap biraz gecikebilir. Yine de yazabilirsin.";
        private const string InputPrefix = "Sen: ";
        private const int VisibleCharacters = 70;
        private const float BackspaceDelay = 0.4f;
        private const float BackspaceInterval = 0.05f;

        [SerializeField] private OllamaSettings _settings = new OllamaSettings();
        [SerializeField] private Transform _oak;
        [SerializeField] private Transform _player;
        [SerializeField] private OakChatView _view;
        [SerializeField] private ConnectionStatusView _connection;
        [SerializeField] private float _talkDistance = 8f;
        [SerializeField] private float _caretBlinkSeconds = 0.5f;

        private readonly ChatInputBuffer _buffer = new ChatInputBuffer();

        private IEventBus _bus;
        private OakDialogueSession _session;
        private CancellationTokenSource _cancellation;
        private Keyboard _keyboard;
        private bool _open;
        private bool _thinking;
        private bool _warming;
        private float _backspaceHeld;

        public void InjectEventBus(IEventBus eventBus)
        {
            _bus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
            _bus = eventBus;
            _bus?.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
            EnsureSession();
        }

        private void Start()
        {
            RefreshTrust();
            WarmUp();
        }

        private async void WarmUp()
        {
            _warming = true;
            var warm = await OllamaWarmup.RunAsync(new UnityWebRequestTransport(), _settings);
            if (this == null)
                return;

            _warming = false;
            Debug.Log($"[Meşe] Model ısıtma: {(warm ? "tamam" : "başarısız")}", this);
        }

        private void OnDestroy()
        {
            if (_keyboard != null)
                _keyboard.onTextInput -= OnTextInput;

            _bus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
            CancelPending();

            if (_open)
                _bus?.Publish(new DialogueFocusChangedEvent(false));
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _view == null)
                return;

            Hook(keyboard);

            if (!_open)
            {
                var near = IsNear();
                _view.SetPrompt(near ? TalkPrompt : string.Empty);
                if (near && keyboard.eKey.wasPressedThisFrame)
                    Open();
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame || !IsNear())
            {
                Close();
                return;
            }

            if (!_thinking)
            {
                HandleBackspace(keyboard);

                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                    Submit();
            }

            ShowInput();
        }

        private void Hook(Keyboard keyboard)
        {
            if (_keyboard == keyboard)
                return;

            if (_keyboard != null)
                _keyboard.onTextInput -= OnTextInput;

            _keyboard = keyboard;
            _keyboard.onTextInput += OnTextInput;
        }

        private void OnTextInput(char character)
        {
            if (_open && !_thinking)
                _buffer.Append(character);
        }

        private void HandleBackspace(Keyboard keyboard)
        {
            var key = keyboard.backspaceKey;
            if (key.wasPressedThisFrame)
            {
                _buffer.Backspace();
                _backspaceHeld = 0f;
            }
            else if (key.isPressed)
            {
                _backspaceHeld += Time.unscaledDeltaTime;
                if (_backspaceHeld >= BackspaceDelay)
                {
                    _buffer.Backspace();
                    _backspaceHeld = BackspaceDelay - BackspaceInterval;
                }
            }
        }

        private bool IsNear()
        {
            if (_oak == null || _player == null)
                return false;

            var offset = _oak.position - _player.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= _talkDistance * _talkDistance;
        }

        private void Open()
        {
            _open = true;
            _buffer.Clear();
            _view.SetPrompt(string.Empty);
            _view.SetOpen(true);
            _view.ShowPlayer(string.Empty);
            _view.ShowReply(_warming ? WakingText : ListeningText);
            RefreshTrust();
            ShowInput();
            _bus?.Publish(new DialogueFocusChangedEvent(true));
        }

        private void Close()
        {
            _open = false;
            CancelPending();
            _thinking = false;
            _buffer.Clear();
            _view.SetThinking(false);
            _view.SetOpen(false);
            _bus?.Publish(new DialogueFocusChangedEvent(false));
        }

        private void ShowInput()
        {
            var caret = _thinking || (int)(Time.unscaledTime / _caretBlinkSeconds) % 2 != 0 ? string.Empty : "|";
            var typed = _buffer.Text;
            if (typed.Length > VisibleCharacters)
                typed = typed.Substring(typed.Length - VisibleCharacters);

            _view.SetInput(InputPrefix + typed + caret);
        }

        private void Submit()
        {
            if (_buffer.IsEmpty)
                return;

            Ask(_buffer.Take());
        }

        private async void Ask(string text)
        {
            CancelPending();
            _cancellation = new CancellationTokenSource();
            var token = _cancellation.Token;

            _thinking = true;
            _view.ShowPlayer(InputPrefix + text);
            _view.ShowReply(string.Empty);
            _view.SetThinking(true);

            OakTurn turn;
            try
            {
                turn = await EnsureSession().TalkAsync(text, token);
            }
            catch (Exception exception)
            {
                if (this == null)
                    return;

                Debug.LogException(exception, this);
                _thinking = false;
                _view.SetThinking(false);
                return;
            }

            if (this == null || token.IsCancellationRequested)
                return;

            _thinking = false;
            _view.SetThinking(false);
            _view.ShowReply(turn.Reply);

            Debug.Log($"[Meşe] Durum: {turn.Status}, eylem: {turn.Action.Name}, güven: {turn.PreviousTrust} -> {turn.CurrentTrust}", this);

            if (_connection != null)
                _connection.ShowDialogueStatus(turn.Status);

            RefreshTrust();

            if (_bus == null)
                return;

            if (turn.TrustChanged)
                _bus.Publish(new TrustChangedEvent(turn.PreviousTrust, turn.CurrentTrust, turn.CurrentTrust - turn.PreviousTrust));

            turn.Action.Execute(_bus);
        }

        private void OnDeedRecorded(DeedRecordedEvent recorded)
        {
            EnsureSession().AddDeed(recorded.Description);
        }

        private OakDialogueSession EnsureSession()
        {
            return _session ??= new OakDialogueSession(new OakDialogueClient(new UnityWebRequestTransport(), _settings));
        }

        private void RefreshTrust()
        {
            if (_view == null)
                return;

            var trust = EnsureSession().Trust;
            _view.SetTrust(trust.Current, trust.Min, trust.Max);
        }

        private void CancelPending()
        {
            if (_cancellation == null)
                return;

            _cancellation.Cancel();
            _cancellation.Dispose();
            _cancellation = null;
        }
    }
}
