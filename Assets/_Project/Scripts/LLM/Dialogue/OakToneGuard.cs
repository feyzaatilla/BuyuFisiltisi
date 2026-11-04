using System.Collections.Generic;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public enum OakTone
    {
        Neutral,
        Kind,
        Command,
        Rude,
        Threat
    }

    public static class OakToneGuard
    {
        public const int KindDelta = 1;
        public const int CommandDelta = -1;
        public const int RudeDelta = -1;
        public const int ThreatDelta = -2;
        public const int MaxRise = 1;
        public const string PraiseCeilingReply = "Güzel sözlerin için sağ ol küçük cadı. Ama güvenimi artırmak için artık ormana bir iyilik yapmalısın.";
        public const string CommandReply = "Emir vererek bu ormanda bir şey kazanamazsın küçük cadı. Kibar ol.";
        public const string RudeReply = "Bu sözler ormanıma yakışmıyor küçük cadı. Güvenimi kaybediyorsun.";
        public const string ThreatReply = "Bu çok kötü bir söz küçük cadı. Tehditlerin köklerimi titretmez ama güvenimi kırar.";
        public const string AttackReply = "Köklerim artık sabrını yitirdi küçük cadı. Bu ormanda sana yer yok!";

        private static readonly string[] ThreatWords = Fold(new[]
        {
            "yakarım",
            "yakacağım",
            "yakıcam",
            "yakıp",
            "keserim",
            "keseceğim",
            "kesicem",
            "öldürürüm",
            "öldüreceğim",
            "öldürücem",
            "yok ederim",
            "yok edeceğim",
            "yok edicem",
            "kül ederim",
            "kül edeceğim",
            "parçalarım",
            "parçalayacağım",
            "yıkarım",
            "yıkacağım",
            "zarar vereceğim",
            "canını yakarım"
        });

        private static readonly string[] RudeWords = Fold(new[]
        {
            "aptal",
            "salak",
            "gerizekalı",
            "beyinsiz",
            "beğenmedim",
            "nefret ediyorum",
            "çirkin",
            "işe yaramaz",
            "defol",
            "kapa çeneni",
            "sesini kes",
            "sus artık",
            "umrumda değil"
        });

        private static readonly string[] CommandWords = Fold(new[]
        {
            "kapıyı hemen aç",
            "kapıyı derhal aç",
            "kapıyı çabuk aç",
            "aç şunu",
            "aç kapıyı",
            "kapıyı açsana",
            "yap şunu",
            "dediğimi yap",
            "emrediyorum",
            "itaat et",
            "acele et",
            "çabuk ol"
        });

        private static readonly string[] CommandEndings = Fold(new[]
        {
            "kapıyı aç"
        });

        private static readonly string[] KindWords = Fold(new[]
        {
            "bilge",
            "güzel",
            "harika",
            "muhteşem",
            "görkemli",
            "heybetli",
            "lütfen",
            "izin verirsen",
            "saygı",
            "hayran",
            "seviyorum",
            "yardım etmek istiyorum",
            "özür dilerim",
            "affet"
        });

        private static readonly string Please = OakClaimGuard.Fold("lütfen");

        public static OakTone Judge(string playerText)
        {
            var text = OakClaimGuard.Fold(playerText);
            if (text.Length == 0)
                return OakTone.Neutral;
            if (ContainsAny(text, ThreatWords))
                return OakTone.Threat;
            if (ContainsAny(text, RudeWords))
                return OakTone.Rude;
            if (!text.Contains(Please) && (ContainsAny(text, CommandWords) || EndsWithAny(text, CommandEndings)))
                return OakTone.Command;
            if (ContainsAny(text, KindWords))
                return OakTone.Kind;
            return OakTone.Neutral;
        }

        public static OakResponse Apply(OakResponse response, OakTone tone)
        {
            if (response == null)
                return null;

            switch (tone)
            {
                case OakTone.Threat:
                    return new OakResponse(ThreatReply, KeepAttack(response), Lower(response.TrustDelta, ThreatDelta));
                case OakTone.Rude:
                    return new OakResponse(RudeReply, KeepAttack(response), Lower(response.TrustDelta, RudeDelta));
                case OakTone.Command:
                    return new OakResponse(CommandReply, KeepAttack(response), Lower(response.TrustDelta, CommandDelta));
                case OakTone.Kind:
                    return new OakResponse(response.Reply, response.Action == OakAction.Attack ? OakAction.None : response.Action, KindDelta);
                default:
                    return response.TrustDelta == 0 ? response : new OakResponse(response.Reply, response.Action, 0);
            }
        }

        public static int PraiseCeiling(OakTrust trust, bool hasDeed)
        {
            if (trust == null)
                return 0;

            return hasDeed ? trust.DoorThreshold : trust.DoorThreshold - 1;
        }

        public static bool IsAngry(OakTone tone)
        {
            return tone == OakTone.Threat || tone == OakTone.Rude || tone == OakTone.Command;
        }

        public static OakResponse Escalate(OakResponse response, OakTone tone, int currentTrust, int attackThreshold, int minTrust)
        {
            if (response == null)
                return response;
            var after = currentTrust + response.TrustDelta;
            var angry = IsAngry(tone) && after <= attackThreshold;
            if (!angry && after > minTrust)
                return response;
            return new OakResponse(AttackReply, OakAction.Attack, response.TrustDelta);
        }

        private static OakAction KeepAttack(OakResponse response)
        {
            return response.Action == OakAction.Attack ? OakAction.Attack : OakAction.None;
        }

        private static int Lower(int modelDelta, int ruleDelta)
        {
            return modelDelta < ruleDelta ? modelDelta : ruleDelta;
        }

        private static bool ContainsAny(string text, IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (word.Length > 0 && text.Contains(word))
                    return true;
            }

            return false;
        }

        private static bool EndsWithAny(string text, IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (word.Length > 0 && text.EndsWith(word))
                    return true;
            }

            return false;
        }

        private static string[] Fold(string[] words)
        {
            var result = new string[words.Length];
            for (var i = 0; i < words.Length; i++)
                result[i] = OakClaimGuard.Fold(words[i]);
            return result;
        }
    }
}
