using System;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Ollama
{
    [Serializable]
    public sealed class OllamaSettings
    {
        [SerializeField] private string baseUrl = "http://localhost:11434";
        [SerializeField] private string model = "gemma2:2b";
        [SerializeField, Min(1f)] private float timeoutSeconds = 10f;
        [SerializeField, Range(0f, 1.5f)] private float temperature = 0.7f;

        public OllamaSettings()
        {
        }

        public OllamaSettings(string baseUrl, string model, float timeoutSeconds, float temperature)
        {
            this.baseUrl = baseUrl;
            this.model = model;
            this.timeoutSeconds = timeoutSeconds;
            this.temperature = temperature;
        }

        public string BaseUrl => string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:11434" : baseUrl.TrimEnd('/');
        public string Model => model;
        public float TimeoutSeconds => timeoutSeconds < 1f ? 1f : timeoutSeconds;
        public float Temperature => temperature;
        public string ChatUrl => BaseUrl + "/api/chat";
    }
}
