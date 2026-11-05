using System.Collections.Generic;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakDoorHint
    {
        public const string LockedReply = "O kapıyı yalnızca ben açarım küçük cadı. Ormana iyilik yap, güvenimi kazan ve benden kibarca iste.";
        public const string ReadyReply = "Güvenimi kazandın küçük cadı. Kapıyı açmamı kibarca istemen yeterli.";
        public const string OpenReply = "Güvenimi kazandın küçük cadı. Geç bakalım, kapı açılıyor.";
        public const string UnfinishedReply = "Güvenimi kazandın küçük cadı ama ormana iyiliğin bitmedi.";
        public const string UnfinishedEnding = "Sonra kapıyı açarım.";
        public const string NotYetReply = "Henüz sana yeterince güvenmiyorum küçük cadı. Önce ormana iyilik yap.";

        private static readonly string[] DoorWords = { "kapi", "muhur" };
        private static readonly string[] QuestionWords = { "nasil", "neyapmal", "negerek", "nedemeli", "hangiyol" };
        private static readonly string[] RequestWords = { "acarmisin", "acabilirmisin", "acmani", "acarsan", "acarmiydin", "acabilirsen" };
        private const string DoorOpenEnding = "kapiyiac";

        public static bool IsDoorQuestion(string playerText)
        {
            var text = OakClaimGuard.Fold(playerText);
            return ContainsAny(text, DoorWords) && ContainsAny(text, QuestionWords);
        }

        public static bool HasRequestWord(string playerText)
        {
            return ContainsAny(OakClaimGuard.Fold(playerText), RequestWords);
        }

        public static bool IsDoorRequest(string playerText)
        {
            var text = OakClaimGuard.Fold(playerText);
            if (!ContainsAny(text, DoorWords) || ContainsAny(text, QuestionWords))
                return false;

            return ContainsAny(text, RequestWords) || text.EndsWith(DoorOpenEnding);
        }

        public static string Unfinished(string missingHint)
        {
            return string.IsNullOrEmpty(missingHint)
                ? UnfinishedReply + " " + UnfinishedEnding
                : UnfinishedReply + " " + missingHint + " " + UnfinishedEnding;
        }

        public static OakResponse Request(OakResponse response, bool trusted, IReadOnlyList<string> missingHints = null)
        {
            if (response == null)
                return null;

            if (!trusted)
                return new OakResponse(NotYetReply, OakAction.None, response.TrustDelta);

            if (missingHints != null && missingHints.Count > 0)
                return new OakResponse(Unfinished(missingHints[0]), OakAction.None, response.TrustDelta);

            return new OakResponse(OpenReply, OakAction.OpenDoor, response.TrustDelta);
        }

        public static OakResponse Apply(OakResponse response, bool canOpenDoor)
        {
            if (response == null)
                return null;

            var delta = response.TrustDelta > 0 ? 0 : response.TrustDelta;
            return new OakResponse(canOpenDoor ? ReadyReply : LockedReply, OakAction.GiveHint, delta);
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
    }
}
