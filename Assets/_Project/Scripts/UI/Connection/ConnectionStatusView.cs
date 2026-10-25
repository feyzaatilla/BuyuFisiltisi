using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BuyuFisiltisi.UI.Connection
{
    public sealed class ConnectionStatusView : MonoBehaviour
    {
        [SerializeField] private OllamaConnectionMonitor _monitor;
        [SerializeField] private Text _indicator;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _title;
        [SerializeField] private Text _detail;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Text _retryLabel;
        [SerializeField] private Color _goodColor = new Color(0.55f, 0.9f, 0.55f);
        [SerializeField] private Color _badColor = new Color(0.95f, 0.45f, 0.4f);
        [SerializeField] private Color _neutralColor = new Color(0.9f, 0.85f, 0.6f);

        private void Awake()
        {
            EnsureEventSystem();
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetry);

            if (_monitor == null)
                return;

            _monitor.HealthChanged += OnHealthChanged;
            OnHealthChanged(_monitor.Health);
        }

        private void OnDisable()
        {
            if (_retryButton != null)
                _retryButton.onClick.RemoveListener(OnRetry);

            if (_monitor != null)
                _monitor.HealthChanged -= OnHealthChanged;
        }

        public void ShowDialogueStatus(OakDialogueStatus status)
        {
            Apply(ConnectionMessages.For(status));
        }

        private void OnHealthChanged(OllamaHealth health)
        {
            Apply(ConnectionMessages.For(health, _monitor.Model));
        }

        private void Apply(ConnectionView view)
        {
            if (_indicator != null)
            {
                _indicator.text = view.Indicator;
                _indicator.color = ColorFor(view.Tone);
            }

            if (_panel != null && view.Panel != PanelVisibility.Unchanged)
                _panel.SetActive(view.Panel == PanelVisibility.Shown);

            if (_title != null)
                _title.text = ConnectionMessages.PanelTitle;

            if (_detail != null && !string.IsNullOrEmpty(view.Detail))
                _detail.text = view.Detail;

            if (_retryButton != null)
                _retryButton.interactable = !view.Busy;

            if (_retryLabel != null)
                _retryLabel.text = view.Busy ? ConnectionMessages.BusyLabel : ConnectionMessages.RetryLabel;
        }

        private Color ColorFor(ConnectionTone tone)
        {
            switch (tone)
            {
                case ConnectionTone.Good:
                    return _goodColor;
                case ConnectionTone.Bad:
                    return _badColor;
                default:
                    return _neutralColor;
            }
        }

        private void OnRetry()
        {
            if (_monitor != null)
                _monitor.Retry();
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
