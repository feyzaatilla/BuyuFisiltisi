using System.Text;
using BuyuFisiltisi.LLM.Dialogue;

namespace BuyuFisiltisi.UI.Oak
{
    public sealed class ChatInputBuffer
    {
        private readonly StringBuilder text = new StringBuilder();
        private readonly int maxLength;

        public ChatInputBuffer(int maxLength = OakConversation.MaxPlayerTextLength)
        {
            this.maxLength = maxLength < 1 ? 1 : maxLength;
        }

        public string Text => text.ToString();
        public bool IsEmpty => text.ToString().Trim().Length == 0;

        public bool Append(char character)
        {
            if (char.IsControl(character) || text.Length >= maxLength)
                return false;

            text.Append(character);
            return true;
        }

        public bool Backspace()
        {
            if (text.Length == 0)
                return false;

            text.Length--;
            return true;
        }

        public void Clear()
        {
            text.Length = 0;
        }

        public string Take()
        {
            var value = text.ToString().Trim();
            text.Length = 0;
            return value;
        }
    }
}
