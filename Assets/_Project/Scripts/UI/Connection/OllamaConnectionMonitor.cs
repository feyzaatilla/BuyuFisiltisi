using System;
using System.Threading;
using BuyuFisiltisi.LLM.Ollama;
using UnityEngine;

namespace BuyuFisiltisi.UI.Connection
{
    public sealed class OllamaConnectionMonitor : MonoBehaviour
    {
        [SerializeField] private OllamaSettings _settings = new OllamaSettings();
        [SerializeField] private bool _checkOnStart = true;

        private OllamaHealthCheck _check;
        private CancellationTokenSource _cancellation;

        public event Action<OllamaHealth> HealthChanged;

        public OllamaHealth Health { get; private set; } = OllamaHealth.Unknown;
        public string Model => _settings.Model;

        private void Start()
        {
            if (_checkOnStart)
                Retry();
        }

        private void OnDestroy()
        {
            CancelPending();
        }

        public async void Retry()
        {
            if (Health == OllamaHealth.Checking)
                return;

            CancelPending();
            _cancellation = new CancellationTokenSource();
            var token = _cancellation.Token;

            _check ??= new OllamaHealthCheck(new UnityWebRequestTransport(), _settings);
            SetHealth(OllamaHealth.Checking);

            var result = await _check.CheckAsync(token);
            if (token.IsCancellationRequested)
                return;

            Debug.Log($"[Ollama] Bağlantı durumu: {result} ({_settings.BaseUrl}, {_settings.Model})", this);
            SetHealth(result);
        }

        private void SetHealth(OllamaHealth health)
        {
            if (Health == health)
                return;

            Health = health;
            HealthChanged?.Invoke(health);
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
