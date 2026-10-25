using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Ollama
{
    public static class OllamaChatRequest
    {
        [Serializable]
        private sealed class Options
        {
            public float temperature;
        }

        [Serializable]
        private sealed class Body
        {
            public string model;
            public bool stream;
            public string format;
            public Options options;
            public List<ChatMessage> messages;
        }

        public static string Build(OllamaSettings settings, IEnumerable<ChatMessage> messages)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var body = new Body
            {
                model = settings.Model,
                stream = false,
                format = "json",
                options = new Options { temperature = settings.Temperature },
                messages = new List<ChatMessage>(messages)
            };

            return JsonUtility.ToJson(body);
        }
    }
}
