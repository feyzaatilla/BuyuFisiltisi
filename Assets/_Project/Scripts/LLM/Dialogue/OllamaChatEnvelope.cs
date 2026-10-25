using System;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OllamaChatEnvelope
    {
        [Serializable]
        private sealed class Message
        {
            public string role;
            public string content;
        }

        [Serializable]
        private sealed class Envelope
        {
            public Message message;
            public bool done;
        }

        public static bool TryReadContent(string body, out string content)
        {
            content = null;
            if (string.IsNullOrWhiteSpace(body))
                return false;

            Envelope envelope;
            try
            {
                envelope = JsonUtility.FromJson<Envelope>(body);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (envelope?.message == null || string.IsNullOrWhiteSpace(envelope.message.content))
                return false;

            content = envelope.message.content;
            return true;
        }
    }
}
