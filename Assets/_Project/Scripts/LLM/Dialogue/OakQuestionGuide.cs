using System.Collections.Generic;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakQuestionGuide
    {
        public const string BridgeReply = "Köprüyü bir şalter indirir. Büyü Kitabı'nı Tab ile aç ve Fısıl'a şalteri çekmesini söyle.";
        public const string FireflyReply = "Kazanda Işık Çayı demle. Sonra yaralı ateş böceğine dokun.";
        public const string RootReply = "Kazanda Sessizlik Merhemi demle. Sonra yaralı köküme sür.";
        public const string CloudReply = "Kara bulutların yanına git. Sol tık ya da R ile rüzgâr büyüsü yap.";
        public const string FoxReply = "Gölge Tilki'ye zarar verme. Büyünle sersemlet, sonra yanına gidip onu sev.";
        public const string CaveReply = "Gizli girdabın altında tuzaklı bir mağara var. Tuzaklardan geç ve taş bulmacayı çöz.";
        public const string CauldronReply = "Ormandan çiçek topla. Sonra kazana tıklayıp iksir ya da merhem demle.";
        public const string TrustReply = "Ormana iyilik yap ve benimle kibar konuş, güvenim artar. Emirler ve kaba sözler onu düşürür.";
        public const string AllDoneReply = "Ormana bütün iyiliklerini yaptın küçük cadı. Şimdi güvenimi kazan ve kapıyı kibarca iste.";

        private static readonly Topic[] Topics =
        {
            new Topic(BridgeReply, "köprü", "şalter"),
            new Topic(FireflyReply, "ateş böce", "ışık çay"),
            new Topic(RootReply, "kök", "merhem"),
            new Topic(CloudReply, "bulut", "rüzgâr", "rüzgar"),
            new Topic(FoxReply, "tilki"),
            new Topic(CaveReply, "mağara", "girdap", "bulmaca", "tuzak"),
            new Topic(CauldronReply, "kazana", "kazanı", "kazanda", "iksir", "tarif", "çiçek"),
            new Topic(TrustReply, "güven")
        };

        private static readonly string[] QuestionWords = Fold(new[]
        {
            "nasıl",
            "ne yapmal",
            "ne yapayım",
            "nerede",
            "nereye",
            "ne gerek",
            "ne demeli",
            "nedir",
            "ipucu",
            "yardım et",
            "yardım eder"
        });

        private static readonly string[] TaskWords = Fold(new[]
        {
            "ne yapmal",
            "ne yapayım",
            "ipucu",
            "görev",
            "sırada ne",
            "nereye gitmeli",
            "yardım et",
            "yardım eder",
            "iyilik yap",
            "nasıl yardım",
            "ne yapabilirim"
        });

        public static bool TryAnswer(string playerText, IReadOnlyCollection<string> deeds, bool canOpenDoor, out string reply)
        {
            reply = null;
            if (OakDoorHint.IsDoorQuestion(playerText))
            {
                reply = canOpenDoor ? OakDoorHint.ReadyReply : OakDoorHint.LockedReply;
                return true;
            }

            var text = OakClaimGuard.Fold(playerText);
            if (text.Length == 0 || !ContainsAny(text, QuestionWords) && !ContainsAny(text, TaskWords))
                return false;

            foreach (var topic in Topics)
            {
                if (!ContainsAny(text, topic.Keys))
                    continue;

                reply = topic.Reply;
                return true;
            }

            if (!ContainsAny(text, TaskWords))
                return false;

            var missing = OakDeedCatalog.MissingHints(deeds);
            reply = missing.Count > 0 ? missing[0] : AllDoneReply;
            return true;
        }

        public static OakResponse Apply(OakResponse response, string reply)
        {
            if (response == null || string.IsNullOrEmpty(reply))
                return response;

            var delta = response.TrustDelta > OakToneGuard.MaxRise ? OakToneGuard.MaxRise : response.TrustDelta;
            return new OakResponse(reply, OakAction.GiveHint, delta);
        }

        private static bool ContainsAny(string text, string[] words)
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

        private sealed class Topic
        {
            public Topic(string reply, params string[] keys)
            {
                Reply = reply;
                Keys = Fold(keys);
            }

            public string Reply { get; }
            public string[] Keys { get; }
        }
    }
}
