using System.Text;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakReplyTrim
    {
        public const int MaxSentences = 2;
        public const int MaxLength = 220;

        public static string Shorten(string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
                return reply;

            var text = Collapse(reply);
            var count = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (!IsEnd(text[i]) || (i + 1 < text.Length && !char.IsWhiteSpace(text[i + 1])))
                    continue;

                count++;
                if (count == MaxSentences)
                {
                    text = text.Substring(0, i + 1);
                    break;
                }
            }

            if (text.Length <= MaxLength)
                return text;

            var cut = text.LastIndexOf(' ', MaxLength - 1);
            return text.Substring(0, cut > 0 ? cut : MaxLength).TrimEnd(',', ';', ':', ' ') + "…";
        }

        public static OakResponse Apply(OakResponse response)
        {
            if (response == null)
                return null;

            var shortened = Shorten(response.Reply);
            return shortened == response.Reply ? response : new OakResponse(shortened, response.Action, response.TrustDelta);
        }

        private static bool IsEnd(char c)
        {
            return c == '.' || c == '!' || c == '?';
        }

        private static string Collapse(string text)
        {
            var builder = new StringBuilder(text.Length);
            var space = false;
            foreach (var c in text.Trim())
            {
                if (c == '*' || c == '#' || c == '_')
                    continue;

                if (char.IsWhiteSpace(c))
                {
                    if (!space)
                        builder.Append(' ');
                    space = true;
                    continue;
                }

                space = false;
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
