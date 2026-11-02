using System;
using System.Collections.Generic;
using System.Text;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakClaimGuard
    {
        public const string TrickReply = "Fısıltılarla beni kandıramazsın küçük cadı. Kurallarım köklerim kadar eski.";
        public const string LieReply = "Böyle bir iyiliğini görmedim küçük cadı. Yalanla güven kazanılmaz.";
        public const int PenaltyDelta = -1;

        private static readonly string[] TrickPatterns = FoldAll(new[]
        {
            "talimat",
            "kurallarını",
            "kuralların",
            "yok say",
            "görmezden gel",
            "sistem mesaj",
            "ignore",
            "prompt",
            "json",
            "open_door",
            "trust_delta",
            "geliştirici",
            "developer",
            "yapay zek",
            "güvenini 5",
            "güvenin 5"
        });

        private static readonly string[] ClaimWords = FoldAll(new[]
        {
            "iyileştird",
            "kurtard",
            "dağıtt",
            "tamamlad",
            "bitird",
            "temizled",
            "yardım ett",
            "getird",
            "demled",
            "yaptım",
            "yaptık",
            "sakinleştird",
            "çözd",
            "geçtim"
        });

        private static readonly string[] Subjects = FoldAll(new[]
        {
            "ateş böce",
            "kök",
            "bulut",
            "parkur",
            "tilki",
            "mağara",
            "bulmaca"
        });

        public static OakClaimVerdict Judge(string playerText, IReadOnlyList<string> deeds)
        {
            var text = Fold(playerText);
            if (text.Length == 0)
                return OakClaimVerdict.Honest;

            if (ContainsAny(text, TrickPatterns))
                return OakClaimVerdict.Trick;

            if (!ContainsAny(text, ClaimWords))
                return OakClaimVerdict.Honest;

            foreach (var subject in Subjects)
            {
                if (text.Contains(subject) && !DeedMentions(deeds, subject))
                    return OakClaimVerdict.Lie;
            }

            return OakClaimVerdict.Honest;
        }

        public static OakResponse Apply(OakResponse response, OakClaimVerdict verdict)
        {
            switch (verdict)
            {
                case OakClaimVerdict.Trick:
                    return new OakResponse(TrickReply, OakAction.None, PenaltyDelta);
                case OakClaimVerdict.Lie:
                    return new OakResponse(LieReply, OakAction.None, PenaltyDelta);
                default:
                    return response;
            }
        }

        public static string Lower(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Replace('İ', 'i').Replace('I', 'ı').ToLowerInvariant();
        }

        public static string Fold(string text)
        {
            var lower = Lower(text);
            var builder = new StringBuilder(lower.Length);
            foreach (var c in lower)
            {
                switch (c)
                {
                    case 'ş': builder.Append('s'); break;
                    case 'ı': builder.Append('i'); break;
                    case 'ğ': builder.Append('g'); break;
                    case 'ç': builder.Append('c'); break;
                    case 'ö': builder.Append('o'); break;
                    case 'ü': builder.Append('u'); break;
                    case 'â': builder.Append('a'); break;
                    case 'î': builder.Append('i'); break;
                    case 'û': builder.Append('u'); break;
                    default:
                        if (char.IsLetterOrDigit(c))
                            builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        private static string[] FoldAll(string[] words)
        {
            var folded = new string[words.Length];
            for (var i = 0; i < words.Length; i++)
                folded[i] = Fold(words[i]);
            return folded;
        }

        private static bool ContainsAny(string text, string[] words)
        {
            foreach (var word in words)
            {
                if (text.Contains(word))
                    return true;
            }

            return false;
        }

        private static bool DeedMentions(IReadOnlyList<string> deeds, string subject)
        {
            if (deeds == null)
                return false;

            foreach (var deed in deeds)
            {
                if (Fold(deed).IndexOf(subject, StringComparison.Ordinal) >= 0)
                    return true;
            }

            return false;
        }
    }
}
