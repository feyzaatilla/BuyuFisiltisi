using System;
using System.Collections.Generic;
using BuyuFisiltisi.LLM.Ollama;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakConversation
    {
        public const int DefaultMaxMessages = 8;
        public const int MaxPlayerTextLength = 200;

        [Serializable]
        private sealed class Payload
        {
            public string reply;
            public string action;
            public int trust_delta;
        }

        private readonly List<ChatMessage> history = new List<ChatMessage>();
        private readonly int maxMessages;

        public OakConversation(int maxMessages = DefaultMaxMessages)
        {
            this.maxMessages = maxMessages < 2 ? 2 : maxMessages;
        }

        public int Count => history.Count;
        public IReadOnlyList<ChatMessage> History => history;

        public static string Normalize(string playerText)
        {
            if (string.IsNullOrWhiteSpace(playerText))
                return string.Empty;

            var text = playerText.Trim().Replace('\n', ' ').Replace('\r', ' ');
            return text.Length > MaxPlayerTextLength ? text.Substring(0, MaxPlayerTextLength) : text;
        }

        public IReadOnlyList<ChatMessage> BuildRequest(string systemPrompt, string playerText)
        {
            var text = Normalize(playerText);
            if (text.Length == 0)
                throw new ArgumentException("Cadının sözü boş olamaz.", nameof(playerText));

            var messages = new List<ChatMessage>(history.Count + 2) { ChatMessage.FromSystem(systemPrompt ?? string.Empty) };
            messages.AddRange(history);
            messages.Add(ChatMessage.FromUser(text));
            return messages;
        }

        public void Record(string playerText, OakResponse response)
        {
            var text = Normalize(playerText);
            if (text.Length == 0 || response == null)
                return;

            var payload = new Payload
            {
                reply = response.Reply,
                action = OakPersona.ActionName(response.Action),
                trust_delta = response.TrustDelta
            };

            history.Add(ChatMessage.FromUser(text));
            history.Add(ChatMessage.FromAssistant(JsonUtility.ToJson(payload)));

            while (history.Count > maxMessages)
                history.RemoveRange(0, 2);
        }

        public void Clear()
        {
            history.Clear();
        }
    }
}
