using UnityEngine;
using UnityEngine.UI;

namespace BuyuFisiltisi.UI.Oak
{
    public sealed class OakChatView : MonoBehaviour
    {
        private const string ThinkingLabel = "Yaşlı Meşe düşünüyor";
        private const float DotSeconds = 0.4f;

        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _playerText;
        [SerializeField] private Text _replyText;
        [SerializeField] private Text _thinkingText;
        [SerializeField] private Text _inputText;
        [SerializeField] private Text _promptText;
        [SerializeField] private Text _trustText;

        private bool _thinking;

        private void Awake()
        {
            SetOpen(false);
            SetThinking(false);
            SetPrompt(string.Empty);
        }

        private void Update()
        {
            if (!_thinking || _thinkingText == null)
                return;

            var dots = 1 + (int)(Time.unscaledTime / DotSeconds) % 3;
            _thinkingText.text = ThinkingLabel + new string('.', dots);
        }

        public void SetOpen(bool open)
        {
            if (_panel != null)
                _panel.SetActive(open);
        }

        public void SetPrompt(string text)
        {
            if (_promptText == null)
                return;

            _promptText.text = text ?? string.Empty;
            _promptText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void SetInput(string text)
        {
            if (_inputText != null)
                _inputText.text = text ?? string.Empty;
        }

        public void ShowPlayer(string text)
        {
            if (_playerText != null)
                _playerText.text = text ?? string.Empty;
        }

        public void ShowReply(string text)
        {
            if (_replyText != null)
                _replyText.text = text ?? string.Empty;
        }

        public void SetThinking(bool thinking)
        {
            _thinking = thinking;

            if (_thinkingText == null)
                return;

            _thinkingText.gameObject.SetActive(thinking);
            if (thinking)
                _thinkingText.text = ThinkingLabel + ".";
        }

        public void SetTrust(int current, int min, int max)
        {
            if (_trustText != null)
                _trustText.text = $"Meşe'nin güveni: {current} ({min} ile {max} arası)";
        }
    }
}
