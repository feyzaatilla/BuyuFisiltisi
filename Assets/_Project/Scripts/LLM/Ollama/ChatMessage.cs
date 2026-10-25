using System;

namespace BuyuFisiltisi.LLM.Ollama
{
    [Serializable]
    public sealed class ChatMessage
    {
        public const string SystemRole = "system";
        public const string UserRole = "user";
        public const string AssistantRole = "assistant";

        public string role;
        public string content;

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }

        public static ChatMessage FromSystem(string content) => new ChatMessage(SystemRole, content);
        public static ChatMessage FromUser(string content) => new ChatMessage(UserRole, content);
        public static ChatMessage FromAssistant(string content) => new ChatMessage(AssistantRole, content);
    }
}
