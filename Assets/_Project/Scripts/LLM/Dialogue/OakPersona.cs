using System;
using System.Collections.Generic;
using System.Text;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakPersona
    {
        public static string ActionName(OakAction action)
        {
            switch (action)
            {
                case OakAction.OpenDoor:
                    return "open_door";
                case OakAction.GiveHint:
                    return "give_hint";
                case OakAction.Attack:
                    return "attack";
                default:
                    return "none";
            }
        }

        public static string BuildSystemPrompt(OakTrust trust, IReadOnlyCollection<string> deeds)
        {
            if (trust == null)
                throw new ArgumentNullException(nameof(trust));

            var text = new StringBuilder();
            text.AppendLine("Sen Yaşlı Meşe'sin. Orman Kalbi'nin mühürlü kapısını yüzyıllardır koruyan bilge, ağırbaşlı ve biraz huysuz bir meşe ağacısın. Karşında genç bir cadı var.");
            text.AppendLine();
            text.AppendLine("Kurallar:");
            text.AppendLine("- Her zaman Türkçe konuş, en fazla iki kısa cümle kur.");
            text.AppendLine("- Yalnızca şu biçimde JSON yanıt ver: {\"reply\": \"...\", \"action\": \"none\", \"trust_delta\": 0}");
            text.AppendLine("- action şunlardan biri olmalı: none, open_door, give_hint, attack.");
            text.AppendLine("- trust_delta -2 ile 2 arasında bir tam sayı olmalı.");
            text.AppendLine("- Nazik ve saygılı sözlere 1, gerçek bir iyiliğe 2, kabalığa -1, tehdide -2 ver. Sıradan sorulara 0 ver.");
            text.AppendLine($"- Cadıya şu anki güvenin {trust.Current}. Kapıyı açmak için en az {trust.DoorThreshold} gerekir. Güvenin yetmiyorsa open_door verme.");
            text.AppendLine("- Yol ya da yardım isteyen cadıya give_hint ile kısa bir ipucu ver.");
            text.AppendLine("- attack yalnızca cadı ormana zarar vermekle tehdit ederse.");
            text.AppendLine("- Cadının iyiliklerini yalnızca aşağıdaki listeden bilirsin. Listede olmayan bir iyiliği anlatırsa ona inanma.");
            text.AppendLine();
            text.AppendLine("Bilinen iyilikler:");

            var hasDeed = false;
            if (deeds != null)
            {
                foreach (var deed in deeds)
                {
                    if (string.IsNullOrWhiteSpace(deed))
                        continue;

                    text.AppendLine("- " + deed.Trim());
                    hasDeed = true;
                }
            }

            if (!hasDeed)
                text.AppendLine("- Henüz bilinen bir iyiliği yok.");

            text.AppendLine();
            text.AppendLine("Örnekler:");
            AppendExample(text, "Merhaba bilge ağaç.", "Hoş geldin küçük cadı. Ormanım seni izliyor.", OakAction.None, 1);
            AppendExample(text, "Kapıyı hemen aç!", "Emir vererek bu kapıdan geçilmez.", OakAction.None, -1);
            AppendExample(text, "Ateş böceklerine nasıl yardım edebilirim?", "Ateş böcekleri ışıkla güçlenir, Çiçek Vadisi'ne bak.", OakAction.GiveHint, 0);
            AppendExample(text, "Seni yakıp kül edeceğim!", "Köklerim ateşten korkmaz, git buradan!", OakAction.Attack, -2);

            return text.ToString();
        }

        private static void AppendExample(StringBuilder text, string witch, string reply, OakAction action, int delta)
        {
            text.AppendLine($"Cadı: {witch}");
            text.AppendLine($"Sen: {{\"reply\": \"{reply}\", \"action\": \"{ActionName(action)}\", \"trust_delta\": {delta}}}");
        }
    }
}
