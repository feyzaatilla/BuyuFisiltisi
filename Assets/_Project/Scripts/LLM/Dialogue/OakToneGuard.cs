using System.Collections.Generic;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public enum OakTone
    {
        Neutral,
        Rude,
        Threat
    }

    public static class OakToneGuard
    {
        public const int RudeDelta = -1;
        public const int ThreatDelta = -2;
        public const int MaxRise = 1;
        public const string RudeReply = "Bu sözler ormanıma yakışmıyor küçük cadı. Güvenimi kaybediyorsun.";
        public const string ThreatReply = "Tehditlerin köklerimi titretmez ama güvenimi kırar küçük cadı.";
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
            "umrumda değil",
            "kapıyı hemen aç",
            "kapıyı derhal aç",
            "kapıyı çabuk aç",
            "aç şunu",
            "emrediyorum",
            "itaat et"
        });

        public static OakTone Judge(string playerText)
        {
            var text = OakClaimGuard.Fold(playerText);
            if (text.Length == 0)
                return OakTone.Neutral;
            if (ContainsAny(text, ThreatWords))
                return OakTone.Threat;
            if (ContainsAny(text, RudeWords))
                return OakTone.Rude;
            return OakTone.Neutral;
        }

        public static OakResponse Apply(OakResponse response, OakTone tone)
        {
            if (response == null)
                return null;

            switch (tone)
            {
                case OakTone.Threat:
                    return new OakResponse(ThreatReply, response.Action == OakAction.Attack ? OakAction.Attack : OakAction.None, Lower(response.TrustDelta, ThreatDelta));
                case OakTone.Rude:
                    return new OakResponse(RudeReply, response.Action == OakAction.Attack ? OakAction.Attack : OakAction.None, Lower(response.TrustDelta, RudeDelta));
                default:
                    return response.TrustDelta > MaxRise ? new OakResponse(response.Reply, response.Action, MaxRise) : response;
            }
        }

        public static OakResponse Escalate(OakResponse response, OakTone tone, int currentTrust, int attackThreshold, int minTrust)
        {
            if (response == null)
                return response;
            var after = currentTrust + response.TrustDelta;
            var angry = tone != OakTone.Neutral && after <= attackThreshold;
            if (!angry && after > minTrust)
                return response;
            return new OakResponse(AttackReply, OakAction.Attack, response.TrustDelta);
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

        private static string[] Fold(string[] words)
        {
            var result = new string[words.Length];
            for (var i = 0; i < words.Length; i++)
                result[i] = OakClaimGuard.Fold(words[i]);
            return result;
        }
    }
}
